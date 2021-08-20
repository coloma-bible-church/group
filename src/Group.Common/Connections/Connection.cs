namespace Group.Common.Connections
{
    using System;
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
        readonly string _userHeaderName;
        readonly ILogger<Connection> _logger;

        public Connection(
            HttpClient client,
            string serverEndpoint,
            string serverSecret,
            string serverSecretHeaderName,
            string userHeaderName,
            ILogger<Connection> logger)
        {
            _client = client;
            _serverEndpoint = serverEndpoint;
            _serverSecret = serverSecret;
            _serverSecretHeaderName = serverSecretHeaderName;
            _userHeaderName = userHeaderName;
            _logger = logger;
        }

        public async Task<string?> SendAsync(MessageModel message, string user, CancellationToken cancellationToken)
        {
            var response = await _client.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, _serverEndpoint)
                {
                    Content = JsonContent.Create(message),
                    Headers =
                    {
                        { _serverSecretHeaderName, _serverSecret },
                        { _userHeaderName, user }
                    }
                },
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
                return null;
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var guid = Guid.NewGuid().ToString();
            _logger.LogWarning($"Issue {guid}. Failed to send message to {_serverEndpoint}. {response.StatusCode}: {response.ReasonPhrase}. Response content: {content}");
            return guid;
        }
    }
}