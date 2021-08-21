namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class HubMessage
    {
        public HubMessage()
        {}

        public HubMessage(
            string sourceUserName,
            string sourceUserId,
            ConnectionMessage sourceMessage,
            string sourceKind)
        {
            SourceMessage = sourceMessage;
            SourceKind = sourceKind;
            SourceUserId = sourceUserId;
            SourceUserName = sourceUserName;
        }

        public string SourceUserName { get; set; } = string.Empty;

        public string SourceUserId { get; set; } = string.Empty;

        public ConnectionMessage SourceMessage { get; set; } = new();

        public string SourceKind { get; set; } = string.Empty;

        public string TargetUserContact { get; set; } = string.Empty;

        public string TargetUserId { get; set; } = string.Empty;
    }
}