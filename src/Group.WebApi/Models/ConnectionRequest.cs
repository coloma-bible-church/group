namespace Group.WebApi.Models
{
    using System;

    [Serializable]
    public class ConnectionRequest
    {
        public string ConnectionEndpoint { get; set; } = string.Empty;

        public string ConnectionSecret { get; set; } = Guid.NewGuid().ToString();
    }
}