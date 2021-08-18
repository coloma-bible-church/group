namespace Group.WebApi.Services.Twilio.Auth
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using global::Twilio.AspNet.Core;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class TwilioAuthenticationHandler : IAuthenticationHandler
    {
        readonly IConfiguration _configuration;
        string _authToken = null!;
        AuthenticationScheme _scheme = null!;
        HttpContext _context = null!;

        public TwilioAuthenticationHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            await Task.CompletedTask;
            _scheme = scheme;
            _context = context;
            _authToken = _configuration["TWILIO_AUTH_TOKEN"]
                ?? throw new Exception("Missing TWILIO_AUTH_TOKEN environment variable");
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            await Task.CompletedTask;
            if (!new RequestValidationHelper().IsValidRequest(_context, _authToken))
                return AuthenticateResult.NoResult();
            return AuthenticateResult.Success(new AuthenticationTicket(
                new GenericPrincipal(
                    new GenericIdentity("Twilio", _scheme.Name),
                    Array.Empty<string>()
                ),
                _scheme.Name
            ));
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            await Task.CompletedTask;
            _context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            await Task.CompletedTask;
            _context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
        }
    }
}