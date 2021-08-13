namespace Group
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Autofac;
    using Functions;
    using MessageSinks;
    using MessageSinks.Azure;
    using Microsoft.Extensions.Logging;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class Module
    {
        static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<SmsFunction>();
            builder.RegisterType<SmsBroadcastMessageSink>().As<IMessageSink>();
            builder.RegisterType<SmsBroadcastMessageSinkOptionsProvider>();
            builder.Register<SmsBroadcastMessageSinkOptions>(c => c.Resolve<SmsBroadcastMessageSinkOptionsProvider>().Get());
        }

        public static async Task UseAsync<T>(Func<T, Task> useAsync, ILogger? logger = null)
            where T : notnull
        {
            var builder = new ContainerBuilder();
            Register(builder);
            if (logger is not null)
                builder.RegisterInstance(logger);
            await using var context = builder.Build();
            var instance = context.Resolve<T>();
            await useAsync(instance);
        }
    }
}