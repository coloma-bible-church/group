namespace Group.Models
{
    using System;

    public class Message
    {
        public Message(
            string from,
            string fromFriendly,
            string content,
            DateTimeOffset time)
        {
            From = from;
            FromFriendly = fromFriendly;
            Content = content;
            Time = time;
        }

        public string From { get; }
        public string FromFriendly { get; }
        public string Content { get; }
        public DateTimeOffset Time { get; }
    }
}