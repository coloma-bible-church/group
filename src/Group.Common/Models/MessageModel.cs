namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class MessageModel
    {
        public MessageModel()
        {}

        public MessageModel(string from, string body)
        {
            From = from;
            Body = body;
        }

        public string Body { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;
    }
}