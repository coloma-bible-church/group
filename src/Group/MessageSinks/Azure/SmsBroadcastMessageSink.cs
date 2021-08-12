namespace Group.MessageSinks.Azure
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Communication.Sms;
    using Microsoft.Extensions.Options;
    using Models;

    [Register(typeof(IMessageSink))]
    public class SmsBroadcastMessageSink : IMessageSink
    {
        readonly SmsBroadcastMessageSinkOptions _options;

        public SmsBroadcastMessageSink(IOptions<SmsBroadcastMessageSinkOptions> options)
        {
            _options = options.Value;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var client = new SmsClient(_options.ConnectionString);
            await client.SendAsync(
                from: _options.ServiceNumber,
                to: "+15028226975",
                message: message.Content,
                cancellationToken: cancellationToken
            );
        }
    }
}