// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

namespace Group.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using MessageSinks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.EventGrid;
    using Microsoft.Extensions.Logging;
    using Models;
    using Models.Sms;

    public class SmsFunction
    {
        readonly IEnumerable<IMessageSink> _messageSinks;

        public SmsFunction(IEnumerable<IMessageSink> messageSinks)
        {
            _messageSinks = messageSinks;
        }

        [UsedImplicitly]
        [FunctionName("sms")]
        public static async Task Run([EventGridTrigger]SmsReceivedEvent e, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                if (e.Data is not { From: {} from, Message: {} content, ReceivedTimestamp: {} time, To: {} to })
                    throw new Exception("Bad request");
                var message = new Message(
                    from: from,
                    fromFriendly: Guid.NewGuid().ToString(),
                    content: content,
                    time: time,
                    to: to
                );
                await Module.UseAsync(
                    async (SmsFunction function) => await function.RunAsync(message, cancellationToken),
                    logger
                );
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Unhandled exception");
                throw new Exception();
            }
        }

        public async Task RunAsync(Message message, CancellationToken cancellationToken)
        {
            foreach (var sink in _messageSinks)
            {
                await sink.HandleAsync(message, cancellationToken);
            }
        }
    }
}
