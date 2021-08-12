namespace Group.Models.Sms
{
    using System;

    [Serializable]
    public class SmsReceivedEvent
    {
        public SmsReceivedEventData? Data { get; set; }
    }
}