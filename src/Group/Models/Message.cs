namespace Group.Models
{
    using System;

    public class Message
    {
        public Message(
            string from,
            string fromFriendly,
            string content,
            DateTimeOffset time,
            string to)
        {
            From = from;
            FromFriendly = fromFriendly;
            Content = content;
            Time = time;
            To = to;
        }

        public string From { get; }
        public string FromFriendly { get; }
        public string Content { get; }
        public DateTimeOffset Time { get; }
        public string To { get; }
    }
}