namespace Group.MessageSinks.Azure
{
    using System;

    [Serializable]
    public class SmsBroadcastMessageSinkOptions
    {
        public const string SectionName = "SMS";

        public string ConnectionString { get; set; } = "MISSING_CONNECTION_STRING";
        public string ServiceNumber { get; set; } = "MISSING_SERVICE_NUMBER";
    }
}