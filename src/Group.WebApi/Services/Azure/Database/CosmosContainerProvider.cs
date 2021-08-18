namespace Group.WebApi.Services.Azure.Database
{
    using System;
    using Microsoft.Azure.Cosmos;

    public class CosmosContainerProvider
    {
        readonly CosmosClient _client;

        public CosmosContainerProvider(CosmosClient client)
        {
            _client = client;
        }

        static string GetDatabaseId() => GetEnvironmentVariable("DB_DB_ID");

        static string GetEnvironmentVariable(string name) =>
            Environment
                .GetEnvironmentVariables()[name]
                as string ?? throw new Exception($"Cannot find {name} environment variable");

        public Container GetIdentities() =>
            _client.GetContainer(
                GetDatabaseId(),
                GetEnvironmentVariable("DB_CONTAINER_IDENTITIES")
            );

        public Container GetContacts() =>
            _client.GetContainer(
                GetDatabaseId(),
                GetEnvironmentVariable("DB_CONTAINER_CONTACTS")
            );
    }
}