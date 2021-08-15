namespace Group.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using JetBrains.Annotations;
    using MessageSinks;
    using Microsoft.Azure.EventGrid.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.EventGrid;
    using Microsoft.Extensions.Logging;
    using Models;
    using Models.Sms;
    using Models.Users;
    using Newtonsoft.Json;

    public class SmsFunction
    {
        readonly IEnumerable<IMessageSink> _messageSinks;
        readonly UserRepository _userRepository;
        readonly ILogger _logger;

        public SmsFunction(
            IEnumerable<IMessageSink> messageSinks,
            UserRepository userRepository,
            ILogger logger)
        {
            _messageSinks = messageSinks;
            _userRepository = userRepository;
            _logger = logger;
        }

        [UsedImplicitly]
        [FunctionName("sms")]
        public static async Task Run([EventGridTrigger]EventGridEvent e, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                if (JsonConvert.DeserializeObject<SmsReceivedEventData>(e.Data.ToString()) is not { From: {} from, Message: {} content, ReceivedTimestamp: {} time })
                    throw new Exception("Missing fields");
                var message = new Message(
                    from: from,
                    fromFriendly: Guid.NewGuid().ToString(),
                    content: content,
                    time: time
                );
                using var function = IoC.Get<SmsFunction>(
                    builder => builder.RegisterInstance(logger)
                );
                await function.Value.RunAsync(message, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Unhandled exception");
                throw new Exception();
            }
        }

        public async Task RunAsync(Message message, CancellationToken cancellationToken)
        {
            if (await _userRepository.FindUserIdAsync(message.From, cancellationToken) is not {} userId)
            {
                _logger.LogWarning($"Cannot find user ID corresponding to phone number {message.From}. Creating a user account now...");
                userId = await _userRepository.CreateUserAsync(
                    new UserModel
                    {
                        PhoneNumber = message.From
                    },
                    cancellationToken
                );
            }
            _logger.LogInformation($"Sending message from {userId} (phone)");
            foreach (var sink in _messageSinks)
            {
                await sink.HandleAsync(message, cancellationToken);
            }
        }
    }
}
