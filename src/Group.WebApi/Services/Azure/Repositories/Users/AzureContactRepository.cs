namespace Group.WebApi.Services.Azure.Repositories.Users
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Models;
    using Newtonsoft.Json;

    public class AzureContactRepository
    {
        [Serializable]
        class AzureContactModel
        {
            static readonly Regex IdRegex = new(@"([^:\s]+):([\S]+)");

            /// <summary>
            /// The contact kind concatenated with the contact value
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            /// <summary>
            /// The user ID
            /// </summary>
            [JsonProperty("userId")]
            public string? UserId { get; set; }

            public static string MakeId(string kind, string value)
            {
                if (string.IsNullOrWhiteSpace(kind) || kind.Length < 1)
                    throw new ArgumentException("Not enough characters", nameof(kind));
                if (string.IsNullOrWhiteSpace(value) || value.Length < 1)
                    throw new ArgumentException("Not enough characters", nameof(value));
                return $"{kind}:{value}";
            }

            public static bool TryParseId(string id, out string kind, out string value)
            {
                var match = IdRegex.Match(id);
                if (!match.Success)
                {
                    kind = string.Empty;
                    value = string.Empty;
                    return false;
                }
                kind = match.Groups[1].Value;
                value = match.Groups[2].Value;
                return true;
            }
        }

        readonly Container _container;

        public AzureContactRepository(Container container)
        {
            _container = container;
        }

        public async Task<bool> CheckAsync(IEnumerable<ContactModel> contacts, CancellationToken cancellationToken)
        {
            var contactKeys = contacts
                .Select(
                    x =>
                    {
                        var id = AzureContactModel.MakeId(x.Kind, x.Value);
                        return (id, new PartitionKey(id));
                    })
                .ToList();
            var existingContacts = await _container.ReadManyItemsAsync<AzureContactModel>(
                contactKeys,
                cancellationToken: cancellationToken
            );
            return existingContacts.Count > 0;
        }

        public async Task CreateAsync(
            string kind,
            string value,
            string userId,
            CancellationToken cancellationToken)
        {
            var azureModel = new AzureContactModel
            {
                Id = AzureContactModel.MakeId(kind, value),
                UserId = userId
            };
            await _container.CreateItemAsync(
                azureModel,
                new PartitionKey(azureModel.Id),
                cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(IEnumerable<ContactModel> contacts, CancellationToken cancellationToken)
        {
            foreach (var contact in contacts)
            {
                var contactId = AzureContactModel.MakeId(contact.Kind, contact.Value);
                await _container.DeleteItemAsync<AzureContactModel>(
                    contactId,
                    new PartitionKey(contactId),
                    cancellationToken: cancellationToken
                );
            }
        }

        public async IAsyncEnumerable<ContactModel> GetContactsAsync(string userId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var contact in _container
                .GetItemQueryIterator<AzureContactModel>(
                    new QueryDefinition("select * from c where c.userId = @userId")
                        .WithParameter("@userId", userId)
                )
                .ToAsyncEnumerable(cancellationToken))
            {
                if (!AzureContactModel.TryParseId(contact.Id, out var kind, out var value))
                {
                    await _container.DeleteItemAsync<AzureContactModel>(
                        contact.Id,
                        new PartitionKey(contact.Id),
                        cancellationToken: cancellationToken
                    );
                    continue;
                }
                yield return new ContactModel
                {
                    Kind = kind,
                    Value = value
                };
            }
        }

        public async Task<string?> GetIdFromContact(ContactModel contact, CancellationToken cancellationToken)
        {
            try
            {
                var contactId = AzureContactModel.MakeId(contact.Kind, contact.Value);
                var model = await _container
                    .ReadItemAsync<AzureContactModel>(
                        contactId,
                        new PartitionKey(contactId),
                        cancellationToken: cancellationToken
                    );
                return model.Resource.UserId;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateContactsAsync(string userId, IEnumerable<ContactModel> contacts, CancellationToken cancellationToken)
        {
            // See what contacts they already have
            var idToContactMap = new Dictionary<string, AzureContactModel>();
            await foreach (var contact in _container
                .GetItemQueryIterator<AzureContactModel>(
                    new QueryDefinition("select * from c where c.userId = @userId")
                        .WithParameter("@userId", userId)
                )
                .ToAsyncEnumerable(cancellationToken))
            {
                idToContactMap[contact.Id] = contact;
            }

            // Add new contacts
            foreach (var contact in contacts)
            {
                var contactId = AzureContactModel.MakeId(contact.Kind, contact.Value);
                if (idToContactMap.Remove(contactId))
                    continue;
                if (await GetIdFromContact(contact, cancellationToken) is not null)
                    throw new Exception("At least one of this user's new contacts is already in use by another user");
                await CreateAsync(contact.Kind, contact.Value, userId, cancellationToken);
            }

            // Delete old unused contacts
            foreach (var contactId in idToContactMap.Keys)
            {
                await _container.DeleteItemAsync<AzureContactModel>(contactId, new PartitionKey(contactId), cancellationToken: cancellationToken);
            }
        }
    }
}