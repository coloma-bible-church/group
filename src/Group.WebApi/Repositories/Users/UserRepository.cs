namespace Group.WebApi.Repositories.Users
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public abstract class UserRepository : IRepository<UserModel>
    {
        public abstract Task<string> CreateAsync(UserModel model, CancellationToken cancellationToken);

        public abstract Task DeleteAsync(string id, CancellationToken cancellationToken);

        public abstract Task<string[]> GetContactKindsAsync(CancellationToken cancellationToken);

        public abstract Task<string[]> GetContactsByKind(string kind, CancellationToken cancellationToken);

        public abstract Task<string?> GetIdFromContactAsync(ContactModel contact, CancellationToken cancellationToken);

        public abstract Task<string[]> GetIdsAsync(CancellationToken cancellationToken);

        public abstract Task<UserModel?> ReadAsync(string id, CancellationToken cancellationToken);

        public abstract Task UpdateAsync(
            string id,
            UserModel model,
            CancellationToken cancellationToken);
    }
}