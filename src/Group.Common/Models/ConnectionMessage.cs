namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class ConnectionMessage
    {
        public ConnectionMessage()
        {}

        public ConnectionMessage(
            string userContact,
            string body,
            string[] medias)
        {
            UserContact = userContact;
            Body = body;
            Medias = medias;
        }

        public string UserContact { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string[] Medias { get; set; } = Array.Empty<string>();
    }
}