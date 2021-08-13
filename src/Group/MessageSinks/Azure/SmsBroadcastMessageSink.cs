namespace Group.MessageSinks.Azure
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Communication.Sms;
    using Models;

    public class SmsBroadcastMessageSink : IMessageSink
    {
        readonly SmsBroadcastMessageSinkOptions _options;

        public SmsBroadcastMessageSink(SmsBroadcastMessageSinkOptions options)
        {
            _options = options;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var client = new SmsClient(_options.ConnectionString);
            await client.SendAsync(
                from: _options.ServiceNumber,
                to: message.From,
                message: message.Content,
                cancellationToken: cancellationToken
            );
        }
    }
}