namespace Group.Common.Connections
{
    using System;
    using Models;

    public class SendResult
    {
        public SendResult(HubResponse hubResponse)
        {
            HubResponse = hubResponse;
        }

        public SendResult(Guid sendErrorId)
        {
            SendErrorId = sendErrorId;
        }

        public Guid? SendErrorId { get; }

        public HubResponse? HubResponse { get; }
    }
}