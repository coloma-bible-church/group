namespace Group.WebApi.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRepository<T>
    {
        Task<string> CreateAsync(T model, CancellationToken cancellationToken);

        Task DeleteAsync(string id, CancellationToken cancellationToken);

        Task<T?> ReadAsync(string id, CancellationToken cancellationToken);

        Task UpdateAsync(string id, T model, CancellationToken cancellationToken);
    }
}