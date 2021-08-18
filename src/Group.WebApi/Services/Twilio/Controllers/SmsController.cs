namespace Group.WebApi.Services.Twilio.Controllers
{
    using System.Threading.Tasks;
    using global::Twilio.AspNet.Common;
    using global::Twilio.AspNet.Core;
    using global::Twilio.TwiML;
    using global::Twilio.TwiML.Messaging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [RequireHttps]
    [Authorize("TWILIO")]
    public class SmsController : ControllerBase
    {
        /// <summary>
        /// Receives an SMS message from Twilio.
        /// </summary>
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