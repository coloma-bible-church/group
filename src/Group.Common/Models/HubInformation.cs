namespace Group.Common.Models
{
    using System;

    /// <summary>
    /// Information provided by the hub.
    /// </summary>
    [Serializable]
    public class HubInformation
    {
        public HubInformation()
        {}

        public HubInformation(
            string sourceKind,
            string userId,
            string userName)
        {
            SourceKind = sourceKind;
            UserId = userId;
            UserName = userName;
        }

        /// <summary>
        /// What kind of connection the <see cref="SourceInformation"/> came from.
        /// </summary>
        public string SourceKind { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user originating the <see cref="SourceInformation"/>.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The human friendly name of the user originating <see cref="SourceInformation"/>.
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}