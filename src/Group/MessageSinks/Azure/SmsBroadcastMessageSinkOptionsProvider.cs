namespace Group.MessageSinks.Azure
{
    using System;

    public class SmsBroadcastMessageSinkOptionsProvider
    {
        public SmsBroadcastMessageSinkOptions Get()
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            var options = new SmsBroadcastMessageSinkOptions();
            if (environmentVariables["CBC_GROUP_SMS_CONNECTION_STRING"] is string connectionString)
                options.ConnectionString = connectionString;
            if (environmentVariables["CBC_GROUP_SMS_SERVICE_NUMBER"] is string serviceNumber)
                options.ServiceNumber = serviceNumber;
            return options;
        }
    }
}