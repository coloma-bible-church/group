namespace Group.WebApi.Services.Azure.Repositories
{
    using System;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Database;
    using Microsoft.Azure.Cosmos;
    using Models;
    using Newtonsoft.Json;
    using WebApi.Repositories.Users;

    public class AzureUserRepository : UserRepository
    {
        [Serializable]
        public class AzureIdentityModel
        {
            [JsonProperty("id")]
            public string? Id { get; set; }

            [JsonProperty("data")]
            public UserModel? Data { get; set; }
        }

        [Serializable]
        class AzureIdModel
        {
            [JsonProperty("id")]
            public string? Id { get; set; }
        }

        [Serializable]
        class AzureKindModel
        {
            [JsonProperty("kind")]
            public string? Kind { get; set; }
        }

        readonly CosmosContainerProvider _containerProvider;
        readonly AzureContactRepository _contactRepository;

        public AzureUserRepository(
            CosmosContainerProvider containerProvider,
            AzureContactRepository contactRepository)
        {
            _containerProvider = containerProvider;
            _contactRepository = contactRepository;
        }

        public override async Task<string> CreateAsync(UserModel user, CancellationToken cancellationToken)
        {
            // Verify that none of the user's contacts are already in use
            if (await _contactRepository.CheckAsync(user.Contacts, cancellationToken))
                throw new Exception("At least one of this user's contacts is already in use");

            // Add the user model
            var userModel = new AzureIdentityModel
            {
                Id = Guid.NewGuid().ToString(),
                Data = user
            };
            await _containerProvider
                .GetIdentities()
                .CreateItemAsync(userModel, cancellationToken: cancellationToken);

            // Add the user's contacts
            foreach (var contact in user.Contacts)
            {
                await _contactRepository.CreateAsync(
                    contact.Kind,
                    contact.Value,
                    userModel.Id,
                    cancellationToken
                );
            }

            return userModel.Id;
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (await ReadAsync(id, cancellationToken) is not {} userModel)
                throw new Exception("This user does not exist");

            // Delete all the contacts for this user
            await _contactRepository.DeleteAsync(userModel.Contacts, cancellationToken);

            // Delete the user
            await _containerProvider
                .GetIdentities()
                .DeleteItemAsync<AzureIdentityModel>(
                    id,
                    new PartitionKey(id),
                    cancellationToken: cancellationToken
                );
        }

        public override async Task<string[]> GetContactKindsAsync(CancellationToken cancellationToken)
        {
            var contactsContainer = _containerProvider.GetContacts();
            return await contactsContainer
                .GetItemQueryIterator<AzureKindModel>(
                    new QueryDefinition("select c.kind from c")
                )
                .ToObservable()
                .Where(x => x.Kind is not null)
                .Select(x => x.Kind!)
                .Distinct()
                .ToArray()
                .ToTask(cancellationToken);
        }

        public override async Task<string[]> GetContactsByKind(string kind, CancellationToken cancellationToken)
        {
            var contactsContainer = _containerProvider.GetContacts();
            return await contactsContainer
                .GetItemQueryIterator<AzureIdModel>(
                    new QueryDefinition("select c.id from c where c.kind = @kind")
                        .WithParameter("@kind", kind)
                )
                .ToObservable()
                .Where(x => x.Id is not null)
                .Select(x => x.Id!)
                .ToArray()
                .ToTask(cancellationToken);
        }

        public override async Task<string?> GetIdFromContactAsync(
            ContactModel contact,
            CancellationToken cancellationToken) => await _contactRepository.GetIdFromContact(contact, cancellationToken);

        public override async Task<string[]> GetIdsAsync(CancellationToken cancellationToken) =>
            await _containerProvider
                .GetIdentities()
                .GetItemQueryIterator<AzureIdModel>(
                    new QueryDefinition("select c.id from c")
                )
                .ToObservable()
                .Where(x => x.Id is not null)
                .Select(x => x.Id!)
                .ToArray()
                .ToTask(cancellationToken);

        public override async Task<UserModel?> ReadAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _containerProvider
                    .GetIdentities()
                    .ReadItemAsync<AzureIdentityModel>(
                        id,
                        new PartitionKey(id),
                        cancellationToken: cancellationToken
                    );
                return response.Resource.Data;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async Task UpdateAsync(
            string id,
            UserModel model,
            CancellationToken cancellationToken)
        {
            if (await ReadAsync(id, cancellationToken) is null)
                throw new Exception("A user with this ID does not exist");

            await _contactRepository.UpdateContactsAsync(
                id,
                model.Contacts,
                cancellationToken
            );

            // Update the user
            await _containerProvider
                .GetIdentities()
                .UpsertItemAsync(
                    new AzureIdentityModel
                    {
                        Id = id,
                        Data = model
                    },
                    cancellationToken: cancellationToken
                );
        }
    }
}