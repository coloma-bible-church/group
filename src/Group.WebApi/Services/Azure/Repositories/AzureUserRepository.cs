namespace Group.WebApi.Services.Azure.Repositories
{
    using System;
    using System.Linq;
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
        class AzureContactModel
        {
            /// <summary>
            /// The contact value
            /// </summary>
            [JsonProperty("id")]
            public string? Id { get; set; }

            /// <summary>
            /// The contact kind
            /// </summary>
            [JsonProperty("kind")]
            public string? Kind { get; set; }

            /// <summary>
            /// The user ID
            /// </summary>
            [JsonProperty("userId")]
            public string? UserId { get; set; }
        }

        [Serializable]
        class AzureUserModel
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

        public AzureUserRepository(CosmosContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
        }

        public override async Task<string> CreateAsync(UserModel user, CancellationToken cancellationToken)
        {
            // Verify that none of the user's contacts are already in use
            var contactsContainer = _containerProvider.GetContacts();
            var contactKeys = user
                .Contacts
                .Where(x => x.Kind is not null && x.Value is not null)
                .Select(x => (x.Value, new PartitionKey(x.Kind)))
                .ToList();
            var existingContacts = await contactsContainer.ReadManyItemsAsync<AzureContactModel>(
                contactKeys,
                cancellationToken: cancellationToken
            );
            if (existingContacts.Count > 0)
                throw new Exception("At least one of this user's contacts is already in use");

            // Add the user model
            var userModel = new AzureUserModel
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
                if (contact.Kind is null || contact.Value is null)
                    continue;
                await contactsContainer.CreateItemAsync(
                    new AzureContactModel
                    {
                        Id = contact.Value,
                        Kind = contact.Kind,
                        UserId = userModel.Id
                    },
                    cancellationToken: cancellationToken
                );
            }

            return userModel.Id;
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (await ReadAsync(id, cancellationToken) is not {} userModel)
                throw new Exception("This user does not exist");

            // Delete all the contacts for this user
            var contactsContainer = _containerProvider.GetContacts();
            foreach (var contact in userModel.Contacts)
            {
                if (contact.Value is null || contact.Kind is null)
                    continue;
                await contactsContainer.DeleteItemAsync<AzureContactModel>(
                    contact.Value,
                    new PartitionKey(contact.Kind),
                    cancellationToken: cancellationToken
                );
            }

            // Delete the user
            await _containerProvider
                .GetIdentities()
                .DeleteItemAsync<AzureUserModel>(
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
            CancellationToken cancellationToken)
        {
            if (contact.Kind is null || contact.Value is null)
                return null;
            try
            {
                var model = await _containerProvider
                    .GetContacts()
                    .ReadItemAsync<AzureContactModel>(
                        contact.Value,
                        new PartitionKey(contact.Kind),
                        cancellationToken: cancellationToken
                    );
                return model.Resource.UserId;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

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
                    .ReadItemAsync<AzureUserModel>(
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

            // Delete stale contacts
            var contactsContainer = _containerProvider.GetContacts();
            var oldContacts = await contactsContainer
                .GetItemQueryIterator<AzureContactModel>(
                    new QueryDefinition("select * from c where c.userId = @userId")
                        .WithParameter("@userId", id)
                )
                .ToObservable()
                .ToList()
                .ToTask(cancellationToken);
            var staleContacts = oldContacts
                .Where(azureContactModel => !model.Contacts.Any(contactModel =>
                    azureContactModel.Kind == contactModel.Kind
                    && azureContactModel.Id == contactModel.Value
                ));
            foreach (var staleContact in staleContacts)
            {
                await contactsContainer.DeleteItemAsync<AzureContactModel>(
                    staleContact.Id,
                    new PartitionKey(staleContact.Kind),
                    cancellationToken: cancellationToken
                );
            }

            // Add new contacts
            var freshContacts = model
                .Contacts
                .Where(contactModel => !oldContacts.Any(azureContactModel =>
                    azureContactModel.Kind == contactModel.Kind
                    && azureContactModel.Id == contactModel.Value
                ));
            foreach (var freshContact in freshContacts)
            {
                if (await GetIdFromContactAsync(freshContact, cancellationToken) is not null)
                    throw new Exception("At least one of the contacts cannot be added because it is already in use by another identity");
                await contactsContainer
                    .CreateItemAsync(
                        new AzureContactModel
                        {
                            Id = freshContact.Value,
                            Kind = freshContact.Kind,
                            UserId = id
                        },
                        cancellationToken: cancellationToken
                    );
            }

            // Update the user
            await _containerProvider
                .GetIdentities()
                .UpsertItemAsync(
                    new AzureUserModel
                    {
                        Id = id,
                        Data = model
                    },
                    cancellationToken: cancellationToken
                );
        }
    }
}