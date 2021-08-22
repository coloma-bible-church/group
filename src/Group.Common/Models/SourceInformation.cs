namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class SourceInformation
    {
        public SourceInformation()
        {}

        public SourceInformation(
            string body,
            string[] medias,
            string userContact)
        {
            Body = body;
            Medias = medias;
            UserContact = userContact;
        }

        /// <summary>
        /// The message body.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// An array of URIs pointing to media attachments.
        /// </summary>
        public string[] Medias { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The connector-specific contact information of the person who sent this message.
        /// </summary>
        public string UserContact { get; set; } = string.Empty;
    }
}