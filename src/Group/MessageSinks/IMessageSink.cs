namespace Group.MessageSinks
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IMessageSink
    {
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }
}