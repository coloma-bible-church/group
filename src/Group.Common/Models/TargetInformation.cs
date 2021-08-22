namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class TargetInformation
    {
        public TargetInformation()
        {}

        public TargetInformation(
            string userContact,
            string userId,
            string userName)
        {
            UserContact = userContact;
            UserId = userId;
            UserName = userName;
        }

        /// <summary>
        /// The connector-specific contact information of the person who should get this message.
        /// </summary>
        public string UserContact { get; set; } = string.Empty;

        /// <summary>
        /// The user ID of the person who should get this message.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The friendly human readable name of the person who should get this message.
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}