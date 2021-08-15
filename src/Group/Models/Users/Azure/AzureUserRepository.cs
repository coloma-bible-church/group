namespace Group.Models.Users.Azure
{
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Group.Azure;
    using Microsoft.Azure.Cosmos;

    public class AzureUserRepository : UserRepository
    {
        readonly CosmosClient _client;
        readonly string _databaseId;
        readonly string _containerId;

        public AzureUserRepository(
            CosmosClient client,
            string databaseId,
            string containerId)
        {
            _client = client;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public override async Task<string> CreateUserAsync(UserModel user, CancellationToken cancellationToken)
        {
            var azureModel = new AzureUserModel
            {
                Model = user
            };
            var container = GetContainer();
            await container.CreateItemAsync(azureModel, cancellationToken: cancellationToken);
            return azureModel.Id;
        }

        public override async Task<string?> FindUserIdAsync(string phoneNumber, CancellationToken cancellationToken) =>
            await GetContainer()
                .GetItemQueryIterator<string>(
                    new QueryDefinition("select c.id from c where c.model.phone == @phone")
                        .WithParameter("@phone", phoneNumber)
                )
                .AsObservable()
                .FirstOrDefaultAsync()
                .ToTask(cancellationToken);

        Container GetContainer() => _client.GetContainer(_databaseId, _containerId);
    }
}