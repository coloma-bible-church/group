namespace Group.Hub.Repositories.Connections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public abstract class ConnectionsRepository
    {
        public abstract Task<ConnectionModel> CreateAsync(
            string kind,
            ConnectionRequest request,
            CancellationToken cancellationToken);

        public abstract Task<bool> DeleteAsync(string kind, CancellationToken cancellationToken);

        public abstract Task<ConnectionModel?> ReadAsync(string kind, CancellationToken cancellationToken);

        public abstract Task<ConnectionModel?> ReadByServerSecretAsync(
            string serverSecret,
            CancellationToken cancellationToken);

        public abstract Task<string[]> ReadKindsAsync(CancellationToken cancellationToken);
    }
}