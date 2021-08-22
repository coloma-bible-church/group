namespace Group.Common.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    public interface IConnector
    {
        Task<ActionResult> ReceiveFromHubAsync(MessageFromHubToConnector model, string connectionSecret);
    }
}