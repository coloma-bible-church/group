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
            string body,
            string[] medias)
        {
            User = user;
            Body = body;
            Medias = medias;
        }

        public string User { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string[] Medias { get; set; } = Array.Empty<string>();
    }
}