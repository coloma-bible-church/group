namespace Group.Tests.Functions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Group.Functions;
    using MessageSinks;
    using Models;
    using Moq;
    using Xunit;

    public class SmsFunctionClass
    {
        public class RunAsyncMethodShould
        {
            [Fact]
            public async Task PassMessageToAllSinks()
            {
                var sink = new Mock<IMessageSink>();
                sink
                    .Setup(x => x.HandleAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                var function = new SmsFunction(new[]
                {
                    sink.Object
                });
                var message = new Message(
                    from: Guid.NewGuid().ToString(),
                    fromFriendly: Guid.NewGuid().ToString(),
                    content: Guid.NewGuid().ToString(),
                    time: DateTimeOffset.Now,
                    to: Guid.NewGuid().ToString()
                );

                await function.RunAsync(message, CancellationToken.None);

                sink.Verify(x => x.HandleAsync(message, CancellationToken.None), Times.Once);
            }
        }
    }
}