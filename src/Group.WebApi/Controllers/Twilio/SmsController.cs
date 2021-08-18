namespace Group.WebApi.Controllers.Twilio
{
    using System.Threading.Tasks;
    using global::Twilio.AspNet.Common;
    using global::Twilio.AspNet.Core;
    using global::Twilio.TwiML;
    using global::Twilio.TwiML.Messaging;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [RequireHttps]
    public class SmsController : ControllerBase
    {
        [HttpPost]
        [Route("api/v1/twilio/sms")]
        public async Task<TwiMLResult> Post([FromForm] SmsRequest request)
        {
            await Task.CompletedTask;
            return new TwiMLResult(
                new MessagingResponse()
                .Append(
                    new Message()
                        .Body("Thank you!")
                )
            );
        }
    }
}