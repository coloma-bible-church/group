namespace Group.WebApi.Controllers.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Models;
    using Repositories.Connections;
    using Repositories.Users;

    [ApiController]
    [Route("api/v1/connections")]
    [Authorize("SECRET")]
    public class ConnectionsController : ControllerBase
    {
        const string ConnectionSecretHeaderName = "Connection-Secret";
        const string ConnectionUserHeaderName = "Connection-User";
        const string ServerSecretHeaderName = "Server-Secret";
        const string ServerSecretQueryParameterName = "serverSecret";
        const string UserQueryParameterName = "user";

        readonly ConnectionsRepository _connectionsRepository;
        readonly UsersRepository _usersRepository;
        readonly HttpClient _httpClient;
        readonly ILogger<ConnectionsController> _logger;
        readonly LinkGenerator _linkGenerator;

        public ConnectionsController(
            ConnectionsRepository connectionsRepository,
            UsersRepository usersRepository,
            HttpClient httpClient,
            ILogger<ConnectionsController> logger,
            LinkGenerator linkGenerator)
        {
            _connectionsRepository = connectionsRepository;
            _usersRepository = usersRepository;
            _httpClient = httpClient;
            _logger = logger;
            _linkGenerator = linkGenerator;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        [HttpGet]
        public async Task<ActionResult> ListKinds()
        {
            var kinds = await _connectionsRepository.ReadKindsAsync(CancellationToken);
            return Ok(kinds.Select(kind => new ResourceModel(
                kind,
                _linkGenerator.GetPathByAction("GetKind", "connections", new { kind })
            )));
        }

        [HttpGet("{kind}")]
        public async Task<ActionResult> GetKind(string kind)
        {
            var connectionModel = await _connectionsRepository.ReadAsync(kind, CancellationToken);
            if (connectionModel is null)
                return NotFound();
            return Ok(connectionModel);
        }

        [HttpPut("{kind}")]
        public async Task<ActionResult> CreateConnection(string kind, ConnectionRequest request)
        {
            if (!Uri.TryCreate(request.ConnectionEndpoint, UriKind.Absolute, out var connectionEndpointUri)
                || connectionEndpointUri.Scheme != "https")
                return BadRequest($"Invalid {nameof(request.ConnectionEndpoint)}. Make sure it is an absolute URL using the https scheme");
            if (await _connectionsRepository.ReadAsync(kind, CancellationToken) is not null)
                return Conflict();
            var connectionModel = await _connectionsRepository.CreateAsync(kind, request,
                CancellationToken);
            return Ok(new
            {
                Endpoint = new
                {
                    Url = _linkGenerator.GetPathByAction("ReceiveMessage", "Connections", new { kind }),
                    UserIdentification = new
                    {
                        HttpHeader = ConnectionUserHeaderName,
                        QueryParameter = UserQueryParameterName
                    }
                },
                Credentials = new
                {
                    HttpHeader = new
                    {
                        Name = ServerSecretHeaderName,
                        Value = connectionModel.ServerSecret
                    },
                    QueryParameter = new
                    {
                        Name = ServerSecretQueryParameterName,
                        Value = connectionModel.ServerSecret
                    }
                }
            });
        }

        [HttpDelete("{kind}")]
        public async Task<ActionResult> DeleteAsync(string kind)
        {
            if (!await _connectionsRepository.DeleteAsync(kind, CancellationToken))
                return NotFound();
            return Ok();
        }

        [HttpPost("{kind}/messages")]
        [AllowAnonymous]
        public async Task<ActionResult> ReceiveMessage(
            string kind,
            MessageModel messageModel,
            [FromQuery(Name = ServerSecretQueryParameterName)] [FromHeader(Name = ServerSecretHeaderName)] string? serverSecret,
            [FromQuery(Name = UserQueryParameterName)] [FromHeader(Name = ConnectionUserHeaderName)] string user)
        {
            // Make sure they're an authorized source for this message
            if (!await IsConnectionAuthorized(serverSecret, kind))
                return Unauthorized();

            // Find the user that this message came from
            var fromId = await _usersRepository
                .GetIdFromContactAsync(
                    new ContactModel
                    {
                        Kind = kind,
                        Value = user
                    },
                    CancellationToken
                );
            if (fromId is null)
                return BadRequest("Invalid user");
            if (await _usersRepository.ReadAsync(fromId, CancellationToken) is not {} fromIdentity)
                return BadRequest("Invalid user");

            // Update the message model with this user's info
            messageModel.From = fromIdentity.Name;

            // Send this message to all other users
            await BroadcastMessageAsync(messageModel, fromId);

            return Ok();
        }

        async Task BroadcastMessageAsync(MessageModel messageModel, string? fromId)
        {
            var userIds = await _usersRepository.GetIdsAsync(CancellationToken);
            var connectionKindToSendDetailsMap = new Dictionary<string, (string SendEndpoint, string ConnectionSecret)>();
            var badConnectionKinds = new HashSet<string>();
            foreach (var userId in userIds)
            {
                if (userId == fromId)
                    continue; // Don't send back to the user who sent it

                var identity = await _usersRepository.ReadAsync(userId, CancellationToken);
                if (identity is null)
                    continue; // This user was just deleted or something

                // Send the message to this user's first form of contact that works
                foreach (var contact in identity.Contacts)
                {
                    var contactKind = contact.Kind;

                    // Check if this is a known-bad connection
                    if (badConnectionKinds.Contains(contactKind))
                        continue;

                    if (!connectionKindToSendDetailsMap.TryGetValue(contactKind, out var sendDetails))
                    {
                        if (await _connectionsRepository.ReadAsync(contactKind, CancellationToken) is not { } connectionModel
                            || !Uri.TryCreate(
                                connectionModel.ConnectionEndpoint,
                                UriKind.Absolute,
                                out var connectionEndpointUri)
                            || connectionEndpointUri.Scheme != "https")
                        {
                            _logger.LogWarning(
                                $"Bad connection to \"{contactKind}\". Make sure it exists, check its public key, and ensure its send endpoint is https");
                            badConnectionKinds.Add(contactKind);
                            continue;
                        }

                        connectionKindToSendDetailsMap[contactKind] = sendDetails = (
                            connectionEndpointUri.ToString(),
                            connectionModel.ConnectionSecret
                        );
                    }

                    // Try POSTing the message through this connection to this user
                    var response = await _httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, sendDetails.SendEndpoint)
                        {
                            Content = JsonContent.Create(messageModel),
                            Headers =
                            {
                                {
                                    ConnectionSecretHeaderName, sendDetails.ConnectionSecret
                                },
                                {
                                    ConnectionUserHeaderName, contact.Value
                                }
                            }
                        },
                        CancellationToken
                    );
                    if (response.IsSuccessStatusCode)
                        break; // That worked; no need to try another contact method for this person

                    if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        // Something is wrong with authorization for this connection. Don't try it again
                        _logger.LogWarning($"Not authorized to POST to \"{sendDetails.SendEndpoint}\"");
                        badConnectionKinds.Add(contactKind);
                    }
                    else
                    {
                        // Something (temporarily?) went wrong
                        _logger.LogWarning($"Failed to POST message to \"{sendDetails.SendEndpoint}\". Status code = {response.StatusCode}");
                    }
                }
            }
        }

        async Task<bool> IsConnectionAuthorized(string? serverSecret, string kind)
        {
            if (HttpContext
                .User
                .Identities
                .Any(x => x.AuthenticationType == "SECRET"))
                return true;
            if (serverSecret is null)
                return false;
            if (await _connectionsRepository.ReadByServerSecretAsync(serverSecret, CancellationToken) is not {} connectionModel)
                return false;
            return connectionModel.Kind == kind;
        }
    }
}