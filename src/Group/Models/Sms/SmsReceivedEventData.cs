namespace Group.Models.Sms
{
    using System;

    [Serializable]
    public class SmsReceivedEventData
    {
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? ReceivedTimestamp { get; set; }
    }
}