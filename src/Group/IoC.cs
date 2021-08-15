namespace Group
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Autofac;
    using Functions;
    using MessageSinks;
    using MessageSinks.Azure;
    using Microsoft.Azure.Cosmos;
    using Models.Users;
    using Models.Users.Azure;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class IoC
    {
        static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<SmsFunction>();
            builder.RegisterType<SmsBroadcastMessageSink>().As<IMessageSink>();
            builder.RegisterType<SmsBroadcastMessageSinkOptionsProvider>();
            builder.Register<SmsBroadcastMessageSinkOptions>(c => c.Resolve<SmsBroadcastMessageSinkOptionsProvider>().Get());
            builder
                .Register<CosmosClient>(_ =>
                {
                    var environmentVariables = Environment.GetEnvironmentVariables();
                    if (environmentVariables["DB_CONNECTION_STRING"] is not string dbConnectionString)
                        throw new Exception("Failed to find DB_CONNECTION_STRING environment variable");
                    return new CosmosClient(dbConnectionString);
                })
                .SingleInstance();
            builder
                .Register<AzureUserRepository>(c =>
                {
                    var environmentVariables = Environment.GetEnvironmentVariables();
                    if (environmentVariables["DB_DB_ID"] is not string databaseId)
                        throw new Exception("Failed to find DB_DB_ID environment variable");
                    if (environmentVariables["DB_USERS_CONTAINER_ID"] is not string containerId)
                        throw new Exception("Failed to find DB_USERS_CONTAINER_ID environment variable");
                    var client = c.Resolve<CosmosClient>();
                    return new AzureUserRepository(
                        client,
                        databaseId,
                        containerId
                    );
                })
                .As<UserRepository>()
                .SingleInstance();
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