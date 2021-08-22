namespace Group.Common.Models
{
    using System;

    /// <summary>
    /// A message that a connector sends to the hub.
    /// </summary>
    [Serializable]
    public class MessageFromConnectorToHub
    {
        public MessageFromConnectorToHub()
        {}

        public MessageFromConnectorToHub(
            SourceInformation sourceInformation)
        {
            SourceInformation = sourceInformation;
        }

        /// <summary>
        /// The message itself.
        /// </summary>
        public SourceInformation SourceInformation { get; set; } = new();
    }
}