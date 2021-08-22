namespace Group.Twilio.Auth
{
    using System.Threading.Tasks;
    using Common.Auth;
    using Common.Configuration;
    using global::Twilio.AspNet.Core;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class TwilioAuthenticationHandler : IAuthenticationHandler
    {
        readonly IConfiguration _configuration;
        readonly AuthenticationHandlerHelper _authenticationHandlerHelper;
        string _authToken = null!;
        AuthenticationScheme _scheme = null!;
        HttpContext _context = null!;

        public TwilioAuthenticationHandler(
            IConfiguration configuration,
            AuthenticationHandlerHelper authenticationHandlerHelper)
        {
            _configuration = configuration;
            _authenticationHandlerHelper = authenticationHandlerHelper;
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            await Task.CompletedTask;
            _scheme = scheme;
            _context = context;
            _authToken = _configuration.GetRequired("TWILIO_AUTH_TOKEN");
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            await Task.CompletedTask;
            if (!new RequestValidationHelper().IsValidRequest(_context, _authToken, false))
                return AuthenticateResult.NoResult();
            return AuthenticateResult.Success(_authenticationHandlerHelper.CreateTicket(_scheme));
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            await _authenticationHandlerHelper.ChallengeAsync(_scheme, _context.Response);
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            await _authenticationHandlerHelper.ForbidAsync(_context.Response);
        }
    }
}