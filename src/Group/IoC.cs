namespace Group
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Autofac;
    using Functions;
    using MessageSinks;
    using MessageSinks.Azure;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class IoC
    {
        static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<SmsFunction>();
            builder.RegisterType<SmsBroadcastMessageSink>().As<IMessageSink>();
            builder.RegisterType<SmsBroadcastMessageSinkOptionsProvider>();
            builder.Register<SmsBroadcastMessageSinkOptions>(c => c.Resolve<SmsBroadcastMessageSinkOptionsProvider>().Get());
        }

        public static Owned<T> Get<T>(Action<ContainerBuilder>? register = null)
            where T : notnull
        {
            var builder = new ContainerBuilder();
            Register(builder);
            register?.Invoke(builder);
            var context = builder.Build();
            try
            {
                var instance = context.Resolve<T>();
                return new Owned<T>(context, instance);
            }
            catch (Exception e)
            {
                context.Dispose();
                throw new Exception("Error while resolving", e);
            }
        }
    }
}