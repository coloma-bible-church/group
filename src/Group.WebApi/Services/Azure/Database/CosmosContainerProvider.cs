namespace Group.WebApi.Services.Azure.Database
{
    using System;
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

        string GetDatabaseId() => GetConfigurationVariable("DB_DB_ID");

        string GetConfigurationVariable(string name) => _configuration[name] ?? throw new Exception($"Cannot find {name} configuration variable");

        public Container GetIdentities() =>
            _client.GetContainer(
                GetDatabaseId(),
                GetConfigurationVariable("DB_CONTAINER_IDENTITIES")
            );

        public Container GetConnections() =>
            _client.GetContainer(
                GetDatabaseId(),
                GetConfigurationVariable("DB_CONTAINER_CONNECTIONS")
            );

        public Container GetContacts() =>
            _client.GetContainer(
                GetDatabaseId(),
                GetConfigurationVariable("DB_CONTAINER_CONTACTS")
            );
    }
}