namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class ConnectionMessage
    {
        public ConnectionMessage()
        {}

        public ConnectionMessage(
            string user,
            string body)
        {
            User = user;
            Body = body;
        }

        public string User { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}