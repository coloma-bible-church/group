namespace Group.Hub.Services.Azure.Repositories.Connections
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Auth;
    using Hub.Repositories.Connections;
    using Microsoft.Azure.Cosmos;
    using Models;
    using Newtonsoft.Json;

    public class AzureConnectionsRepository : ConnectionsRepository
    {
        readonly Container _container;

        [Serializable]
        class AzureConnectionModel
        {
            public AzureConnectionModel()
            {}

            public AzureConnectionModel(
                string kind,
                string connectionSecret,
                string serverSecret,
                string connectionEndpoint)
            {
                Kind = kind;
                ConnectionSecret = connectionSecret;
                ServerSecret = serverSecret;
                ConnectionEndpoint = connectionEndpoint;
            }

            [JsonProperty("id")]
            public string Kind { get; set; } = Guid.NewGuid().ToString();

            /// <summary>
            /// A secret given by the connection which we must provide when we POST to its <see cref="ConnectionEndpoint"/>
            /// </summary>
            [JsonProperty("connectionSecret")]
            public string ConnectionSecret { get; set; } = Guid.NewGuid().ToString();

            [JsonProperty("serverSecret")]
            public string ServerSecret { get; set; } = Guid.NewGuid().ToString();

            [JsonProperty("connectionEndpoint")]
            public string ConnectionEndpoint { get; set; } = string.Empty;
        }

        public AzureConnectionsRepository(Container container)
        {
            _container = container;
        }

        public override async Task<ConnectionModel> CreateAsync(
            string kind,
            ConnectionRequest request,
            CancellationToken cancellationToken)
        {
            if (await ReadAsync(kind, cancellationToken) is not null)
                throw new Exception("Another connection already exists with this name");
            var serverSecret = Guid.NewGuid().ToString();
            await _container.CreateItemAsync(
                new AzureConnectionModel(
                    kind,
                    request.ConnectionSecret,
                    serverSecret,
                    request.ConnectionEndpoint
                ),
                cancellationToken: cancellationToken
            );
            return new ConnectionModel(
                kind,
                request.ConnectionSecret,
                serverSecret,
                request.ConnectionEndpoint
            );
        }

        public override async Task<bool> DeleteAsync(string kind, CancellationToken cancellationToken)
        {
            try
            {
                await _container.DeleteItemAsync<AzureConnectionModel>(
                    kind,
                    new PartitionKey(kind),
                    cancellationToken: cancellationToken
                );
                return true;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public override async Task<ConnectionModel?> ReadAsync(string kind, CancellationToken cancellationToken)
        {
            try
            {
                var azureModel = (await _container.ReadItemAsync<AzureConnectionModel>(
                    kind,
                    new PartitionKey(kind),
                    cancellationToken: cancellationToken
                )).Resource;
                return new ConnectionModel(
                    azureModel.Kind,
                    azureModel.ConnectionSecret,
                    azureModel.ServerSecret,
                    azureModel.ConnectionEndpoint
                );
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async Task<ConnectionModel?> ReadByServerSecretAsync(
            string serverSecret,
            CancellationToken cancellationToken) =>
            await _container
                .GetItemQueryIterator<AzureConnectionModel>()
                .ToAsyncEnumerable(cancellationToken)
                .Where(x => SecureCompare.Compare(x.ServerSecret, serverSecret))
                .Select(x => new ConnectionModel(
                    x.Kind,
                    x.ConnectionSecret,
                    x.ServerSecret,
                    x.ConnectionEndpoint
                ))
                .FirstOrDefaultAsync(cancellationToken);

        public override async Task<string[]> ReadKindsAsync(CancellationToken cancellationToken) =>
            await _container
                .GetItemQueryIterator<AzureConnectionModel>()
                .ToAsyncEnumerable(cancellationToken)
                .Select(x => x.Kind)
                .ToArrayAsync(cancellationToken);
    }
}