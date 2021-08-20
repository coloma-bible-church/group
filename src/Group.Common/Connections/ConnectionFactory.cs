namespace Group.Common.Connections
{
    using System.Net.Http;
    using Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ConnectionFactory
    {
        readonly HttpClient _httpClient;
        readonly IConfiguration _configuration;
        readonly ILogger<Connection> _logger;

        public ConnectionFactory(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<Connection> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public Connection Create() => new(
            _httpClient,
            _configuration.GetRequired("SERVER_ENDPOINT"),
            _configuration.GetRequired("SERVER_SECRET"),
            ConnectionHeaders.ServerSecretHeaderName,
            ConnectionHeaders.ConnectionUserHeaderName,
            _logger
        );
    }
}