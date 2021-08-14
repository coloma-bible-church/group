namespace Group.Functions
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class MessageFunction
    {
        [UsedImplicitly]
        [FunctionName("message")]
        public static async Task<ActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/message")]
            HttpRequest request,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var user = request.HttpContext.User;
            logger.LogInformation($"POSTED by {request.HttpContext.Connection.RemoteIpAddress} {user.Identity.Name}!");
            await Task.CompletedTask;
            return new OkObjectResult(new
            {
                You = user.Identity.Name
            });
        }
    }
}