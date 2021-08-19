namespace Group.WebApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Redirect("/swagger");
        }

        [HttpGet("login")]
        public string GetLogin() => "Hello, world!";
    }
}