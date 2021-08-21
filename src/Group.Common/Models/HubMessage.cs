namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class HubMessage
    {
        public HubMessage()
        {}

        public HubMessage(
            string name,
            ConnectionMessage sourceMessage,
            string sourceKind)
        {
            SourceMessage = sourceMessage;
            SourceKind = sourceKind;
            Name = name;
        }

        public string Name { get; set; } = string.Empty;

        public ConnectionMessage SourceMessage { get; set; } = new();

        public string SourceKind { get; set; } = string.Empty;

        public string TargetUser { get; set; } = string.Empty;
    }
}