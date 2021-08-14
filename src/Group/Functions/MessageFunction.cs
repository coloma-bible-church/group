namespace Group.Functions
{
    using System;
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
            logger.LogInformation($"POSTED by {request.HttpContext.Connection.RemoteIpAddress}!");
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return new OkObjectResult(new
            {
                Status = "success"
            });
        }
    }
}