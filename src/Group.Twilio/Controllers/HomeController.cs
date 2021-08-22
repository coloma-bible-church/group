namespace Group.Twilio.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get() => Redirect("/swagger");
    }
}