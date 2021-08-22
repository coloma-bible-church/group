namespace Group.Twilio.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Auth;
    using Common.Configuration;
    using Common.Connections;
    using Common.Controllers;
    using Common.Models;
    using Common.Uris;
    using global::Twilio.AspNet.Common;
    using global::Twilio.AspNet.Core;
    using global::Twilio.Clients;
    using global::Twilio.Rest.Api.V2010.Account;
    using global::Twilio.TwiML;
    using global::Twilio.TwiML.Messaging;
    using global::Twilio.Types;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [RequireHttps]
    [Route("api/v1/sms")]
    public class SmsController : ControllerBase, IConnector
    {
        readonly Connection _connection;
        readonly ITwilioRestClient _twilioRestClient;
        readonly ILogger<SmsController> _logger;
        readonly IConfiguration _configuration;

        static readonly HashSet<MessageResource.StatusEnum> FinishedStatuses = new()
        {
            MessageResource.StatusEnum.Sent,
            MessageResource.StatusEnum.Received,
            MessageResource.StatusEnum.Delivered,
            MessageResource.StatusEnum.Undelivered,
            MessageResource.StatusEnum.Failed,
            MessageResource.StatusEnum.Canceled
        };

        public SmsController(
            Connection connection,
            ITwilioRestClient twilioRestClient,
            ILogger<SmsController> logger,
            IConfiguration configuration)
        {
            _connection = connection;
            _twilioRestClient = twilioRestClient;
            _logger = logger;
            _configuration = configuration;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        static string FormatMessageBody(HubMessage message)
        {
            var signature = $"--{message.SourceUserName} ({message.SourceMessage.UserContact})";
            var body = message.SourceMessage.Body;
            if (string.IsNullOrWhiteSpace(body))
                return signature;
            signature = "\n" + signature;
            body = Truncate(
                body,
                1600,
                "...(truncated)"
            );
            return body + signature;
        }

        static string Truncate(string value, int maxCharacters, string truncation)
        {
            if (value.Length <= maxCharacters)
                return value;
            return value[..(maxCharacters - truncation.Length)] + truncation;
        }

        [Authorize("CONNECTION")]
        [HttpPost("connection")]
        public async Task<ActionResult> ReceiveFromHubAsync(
            HubMessage message,
            [FromHeader(Name = ConnectionHeaders.ConnectionSecretHeaderName)] // Helps Swagger UI
            string connectionSecret)
        {
            GC.KeepAlive(connectionSecret);

            // Set up the message to send
            var sendOptions = new CreateMessageOptions(new PhoneNumber(message.TargetUserContact))
            {
                Body = FormatMessageBody(message),
                From = new PhoneNumber(
                    _configuration.GetRequired("TWILIO_PHONE_NUMBER")
                ),
                MediaUrl = message
                    .SourceMessage
                    .Medias
                    .Where(UriChecker.IsValidAndSecure)
                    .Select(x => new Uri(x, UriKind.Absolute))
                    .Take(10)
                    .ToList()
            };

            // Send the message
            var response = await MessageResource.CreateAsync(
                sendOptions,
                _twilioRestClient
            );
            var responseSid = response.Sid;
            var accountSid = response.AccountSid;
            response = await MessageResource.FetchAsync(
                new FetchMessageOptions(responseSid)
                {
                    PathAccountSid = accountSid
                },
                _twilioRestClient
            );

            // Wait for a non-pending message status
            for (var i = 0; !FinishedStatuses.Contains(response.Status); ++i)
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (i == 0)
                {}
                else
                {
                    var timespan = i switch
                    {
                        1 => TimeSpan.FromSeconds(0.5),
                        _ => TimeSpan.FromSeconds(1)
                    };
                    await Task.Delay(timespan, CancellationToken);
                }

                response = await MessageResource.FetchAsync(
                    new FetchMessageOptions(responseSid)
                    {
                        PathAccountSid = accountSid
                    },
                    _twilioRestClient
                );
            }

            // Report status
            var blurb = $"{response.Sid}: {response.Status} from {message.SourceUserId} to {message.TargetUserId}";
            if (response.ErrorCode is {} errorCode)
            {
                var errorMessage = $"{blurb}. Error code {errorCode}: {response.ErrorMessage}";
                _logger.LogWarning(errorMessage);
                return Problem(errorMessage);
            }
            _logger.LogInformation(blurb);
            return Ok(new
            {
                From = response.From.ToString(),
                response.Sid,
                Status = response.Status.ToString()
            });
        }

        /// <summary>
        /// Passes an SMS message from Twilio to the connection
        /// </summary>
        [Authorize("TWILIO")]
        [Authorize("SECRET")]
        [HttpPost("twilio")]
        public async Task<IActionResult> ReceiveFromTwilio(
            [FromForm] SmsRequest request,
            [FromForm] int numMedia,
            [FromHeader(Name = SecretHeaderAuthenticationHandler.HeaderName)] string? secret)
        {
            GC.KeepAlive(secret);

            var medias = new string[numMedia];
            {
                var modelStateDictionary = new ModelStateDictionary();
                if (request.SmsSid is null)
                    modelStateDictionary.AddModelError(nameof(request.SmsSid), "Missing value");
                if (request.From is null)
                    modelStateDictionary.AddModelError(nameof(request.From), "Missing value");
                if (!modelStateDictionary.IsValid || request.SmsSid is null || request.From is null)
                    return BadRequest(modelStateDictionary);
                request.Body ??= string.Empty;
                for (var i = 0; i < numMedia; ++i)
                {
                    var mediaUrlKey = $"MediaUrl{i}";
                    var mediaUrlValue = Request.Form[mediaUrlKey].ToString();
                    if (!UriChecker.IsValidAndSecure(mediaUrlValue))
                        modelStateDictionary.AddModelError(mediaUrlKey, "Must be a valid https URI");
                    medias[i] = mediaUrlValue;
                }
            }
            _logger.LogInformation($"Received SMS SID {request.SmsSid} from Twilio with {numMedia} media");

            var message = new ConnectionMessage(
                userContact: request.From,
                body: request.Body,
                medias: medias
            );
            var result = await _connection.SendAsync(message, CancellationToken);
            string? responseBody;
            switch (result)
            {
                case {SendErrorId: not null}:
                    responseBody = $"🖥️😕 There was a problem processing your message. Ask a human to check the logs for {result.SendErrorId}";
                    break;
                case {HubResponse: {Error: HubResponse.ErrorInvalidUser}}:
                    responseBody = "🖥️👋 Thanks for the message, but I don't recognize your number";
                    break;
                case {HubResponse: {Error: not null}}:
                    var guid = Guid.NewGuid();
                    responseBody = $"🖥️😕 There was a problem processing your message. Ask a human to check the logs for {guid}";
                    _logger.LogWarning($"{guid}: SMS SID {request.SmsSid}: Unrecognized hub response error: {result.HubResponse.Error}");
                    break;
                default:
                    responseBody = null;
                    break;
            }

            var messagingResponse = new MessagingResponse();
            if (responseBody is not null)
                messagingResponse.Append(
                    new Message()
                        .Body(responseBody)
                );
            return new TwiMLResult(
                messagingResponse
            );
        }
    }
}