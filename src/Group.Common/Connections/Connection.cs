namespace Group.Common.Connections
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Models;

    public class Connection
    {
        readonly HttpClient _client;
        readonly string _serverEndpoint;
        readonly string _serverSecret;
        readonly string _serverSecretHeaderName;
        readonly ILogger<Connection> _logger;

        public Connection(
            HttpClient client,
            string serverEndpoint,
            string serverSecret,
            string serverSecretHeaderName,
            ILogger<Connection> logger)
        {
            _client = client;
            _serverEndpoint = serverEndpoint;
            _serverSecret = serverSecret;
            _serverSecretHeaderName = serverSecretHeaderName;
            _logger = logger;
        }

        [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
        public async Task<SendResult> SendAsync(ConnectionMessage message, CancellationToken cancellationToken)
        {
            var response = await _client.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, _serverEndpoint)
                {
                    Content = JsonContent.Create(message),
                    Headers =
                    {
                        { _serverSecretHeaderName, _serverSecret }
                    }
                },
                cancellationToken
            );
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var hubResponse = await response.Content.ReadFromJsonAsync<HubResponse>(cancellationToken: cancellationToken);
                if (hubResponse is not null)
                    return new SendResult(hubResponse);

                var guid = Guid.NewGuid();
                _logger.LogWarning($"{guid}: Unable to parse hub response as JSON: {contentString}");
                return new SendResult(guid);
            }
            else
            {
                var content = contentString;
                var guid = Guid.NewGuid();
                _logger.LogWarning($"{guid}: Failed to send message to {_serverEndpoint}. {response.StatusCode}: {response.ReasonPhrase}. Response content: {content}");
                return new SendResult(guid);
            }
        }
    }
}