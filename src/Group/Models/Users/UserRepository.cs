namespace Group.Models.Users
{
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class UserRepository
    {
        public abstract Task<string> CreateUserAsync(UserModel user, CancellationToken cancellationToken);

        public abstract Task<string?> FindUserIdAsync(string phoneNumber, CancellationToken cancellationToken);
    }
}