namespace Group.Common.Models
{
    using System;

    /// <summary>
    /// A message that a hub sends to a connector.
    /// </summary>
    [Serializable]
    public class MessageFromHubToConnector
    {
        public MessageFromHubToConnector()
        {}

        public MessageFromHubToConnector(
            SourceInformation sourceInformation,
            HubInformation hubInformation,
            TargetInformation targetInformation)
        {
            SourceInformation = sourceInformation;
            HubInformation = hubInformation;
            TargetInformation = targetInformation;
        }

        /// <summary>
        /// The <see cref="SourceInformation"/> provided by the originating connector.
        /// </summary>
        public SourceInformation SourceInformation { get; set; } = new();

        /// <summary>
        /// Additional information from the hub about the <see cref="SourceInformation"/>.
        /// </summary>
        public HubInformation HubInformation { get; set; } = new();

        /// <summary>
        /// Information relevant to sending the message to someone via a connector.
        /// </summary>
        public TargetInformation TargetInformation { get; set; } = new();
    }
}