namespace Group.WebApi.Models
{
    using System;

    [Serializable]
    public class MessageModel
    {
        public string Body { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;
    }
}