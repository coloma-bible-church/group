namespace Group.Hub.Models
{
    using System;

    [Serializable]
    public class ConnectionModel
    {
        public ConnectionModel(
            string kind,
            string connectionSecret,
            string serverSecret,
            string connectionEndpoint)
        {
            Kind = kind;
            ConnectionSecret = connectionSecret;
            ServerSecret = serverSecret;
            ConnectionEndpoint = connectionEndpoint;
        }

        public string Kind { get; }

        public string ConnectionSecret { get; }

        public string ServerSecret { get; }

        public string ConnectionEndpoint { get; }
    }
}