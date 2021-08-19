namespace Group.WebApi.Controllers.Auth
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("secret")]
        [Authorize("SECRET")]
        public string AuthenticateWithSecretPolicy([FromForm] string secret)
        {
            GC.KeepAlive(this);
            GC.KeepAlive(secret);
            return "You are logged in";
        }
    }
}