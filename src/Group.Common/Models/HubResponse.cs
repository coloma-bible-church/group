namespace Group.Common.Models
{
    using System;

    [Serializable]
    public class HubResponse
    {
        public const string
            ErrorInvalidUser = "Invalid user";

        public string? Error { get; set; }
    }
}