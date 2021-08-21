namespace Group.Hub.Controllers.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Connections;
    using Common.Models;
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
            var connectionModel = await _connectionsRepository.CreateAsync(
                kind,
                request,
                CancellationToken);
            return Ok(connectionModel);
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
            ConnectionMessage connectionMessage,
            [FromHeader(Name = ConnectionHeaders.ServerSecretHeaderName)] string? serverSecret)
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
                        Value = connectionMessage.User
                    },
                    CancellationToken
                );
            if (fromId is null || await _usersRepository.ReadAsync(fromId, CancellationToken) is not {} fromIdentity)
                return Ok(new HubResponse
                {
                    Error = HubResponse.ErrorInvalidUser
                });

            // Create a hub message model from this connection message model
            var hubMessage = new HubMessage(
                fromIdentity.Name,
                connectionMessage,
                kind);

            _logger.LogInformation($"Received \"{kind}\" message from \"{fromId}\"");

            // Send this message to all other users
            await BroadcastMessageAsync(hubMessage, fromId);

            return Ok(new HubResponse
            {
                Error = null
            });
        }

        async Task BroadcastMessageAsync(HubMessage hubMessage, string fromId)
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
                    hubMessage.TargetUser = contact.Value;
                    var response = await _httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, sendDetails.SendEndpoint)
                        {
                            Content = JsonContent.Create(hubMessage),
                            Headers =
                            {
                                {
                                    ConnectionHeaders.ConnectionSecretHeaderName, sendDetails.ConnectionSecret
                                }
                            }
                        },
                        CancellationToken
                    );

                    var description = $"\"{contactKind}\" message from \"{fromId}\" to \"{userId}\"";
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Sent {description}");
                        break; // That worked; no need to try another contact method for this person
                    }
                    _logger.LogWarning($"Failed to send {description}. {response.StatusCode}: \"{response.ReasonPhrase}\"");
                    if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        // Something is wrong with authorization for this connection. Don't try it again
                        badConnectionKinds.Add(contactKind);
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