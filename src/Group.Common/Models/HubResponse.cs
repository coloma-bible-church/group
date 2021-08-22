namespace Group.Common.Models
{
    using System;

    /// <summary>
    /// The hub's response to a <see cref="MessageFromConnectorToHub"/>.
    /// </summary>
    [Serializable]
    public class HubResponse
    {
        public const string
            ErrorInvalidUser = "Invalid user";

        /// <summary>
        /// Null if there were no problems. Otherwise, a human readable error message describing the problem.
        /// </summary>
        public string? Error { get; set; }
    }
}