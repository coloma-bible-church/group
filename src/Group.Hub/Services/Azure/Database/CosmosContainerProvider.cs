namespace Group.Hub.Services.Azure.Database
{
    using Common.Configuration;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;

    public class CosmosContainerProvider
    {
        readonly CosmosClient _client;
        readonly IConfiguration _configuration;

        public CosmosContainerProvider(
            CosmosClient client,
            IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        string GetDatabaseId() => _configuration.GetRequired("DB_DB_ID");

        public Container GetIdentities() =>
            _client.GetContainer(
                GetDatabaseId(),
                _configuration.GetRequired("DB_CONTAINER_IDENTITIES")
            );

        public Container GetConnections() =>
            _client.GetContainer(
                GetDatabaseId(),
                _configuration.GetRequired("DB_CONTAINER_CONNECTIONS")
            );

        public Container GetContacts() =>
            _client.GetContainer(
                GetDatabaseId(),
                _configuration.GetRequired("DB_CONTAINER_CONTACTS")
            );
    }
}