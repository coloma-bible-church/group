namespace Group.Hub.Services.Azure.Database
{
    using System;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;

    public class CosmosClientFactory
    {
        readonly IConfiguration _configuration;

        public CosmosClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CosmosClient Create()
        {
            var connectionString = _configuration.GetConnectionString("cosmos") ?? throw new Exception("Missing Cosmos DB connection string");
            var client = new CosmosClient(connectionString);
            return client;
        }
    }
}