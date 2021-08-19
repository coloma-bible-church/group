namespace Group.WebApi.Services.Azure.Repositories
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Database;
    using Microsoft.Azure.Cosmos;
    using Models;
    using WebApi.Repositories.Users;

    public class AzureUsersRepository : UsersRepository
    {
        readonly CosmosContainerProvider _containerProvider;
        readonly AzureContactRepository _contactRepository;

        public AzureUsersRepository(
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
                Name = user.Name
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

        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public override async Task<UserModel?> ReadAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var identitiesContainer = _containerProvider.GetIdentities();
                var response = await identitiesContainer
                    .ReadItemAsync<AzureIdentityModel>(
                        id,
                        new PartitionKey(id),
                        cancellationToken: cancellationToken
                    );
                var identityModel = response.Resource;
                if (identityModel.Name is null)
                {
                    identityModel.Name = id;
                    await identitiesContainer.UpsertItemAsync(identityModel, cancellationToken: cancellationToken);
                }
                var contacts = await _contactRepository
                    .GetContactsAsync(id, cancellationToken)
                    .ToArrayAsync();
                return new UserModel
                {
                    Contacts = contacts,
                    Name = identityModel.Name
                };
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
                        Name = model.Name
                    },
                    cancellationToken: cancellationToken
                );
        }
    }
}