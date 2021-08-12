namespace Group.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Auth;
    using MessageSinks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Models.Sms;

    [Route("v1/sms")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        readonly IEnumerable<IMessageSink> _messageSinks;

        public SmsController(
            IEnumerable<IMessageSink> messageSinks)
        {
            _messageSinks = messageSinks;
        }

        [HttpPost]
        [Authorize(Policy = SmsHelpers.CanSendSmsPolicy)]
        public async Task<ActionResult> PostAsync(SmsReceivedEvent smsReceivedEvent)
        {
            if (smsReceivedEvent.Data is not { From: {} from, Message: {} content, ReceivedTimestamp: {} time, To: {} to })
                return BadRequest();
            var message = new Message(
                from: from,
                fromFriendly: Guid.NewGuid().ToString(),
                content: content,
                time: time,
                to: to
            );
            foreach (var sink in _messageSinks)
            {
                await sink.HandleAsync(message, HttpContext.RequestAborted);
            }
            return Ok();
        }
    }
}