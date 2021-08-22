namespace Group.Common.Auth
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class SecretHeaderAuthenticationHandler : IAuthenticationHandler
    {
        public const string HeaderName = "Secret-Header";

        readonly AuthenticationHandlerHelper _authenticationHandlerHelper;
        readonly IConfiguration _configuration;
        AuthenticationScheme _scheme = null!;
        HttpContext _context = null!;
        string _secret = Guid.NewGuid().ToString();

        public SecretHeaderAuthenticationHandler(
            AuthenticationHandlerHelper authenticationHandlerHelper,
            IConfiguration configuration)
        {
            _authenticationHandlerHelper = authenticationHandlerHelper;
            _configuration = configuration;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _scheme = scheme;
            _context = context;
            _secret = _configuration["SECRET_HEADER"] ?? Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            await Task.CompletedTask;
            return SecureCompare.Compare(_context.Request.Headers[HeaderName].ToString(), _secret)
                ? AuthenticateResult.Success(_authenticationHandlerHelper.CreateTicket(_scheme))
                : AuthenticateResult.NoResult();
        }

        public async Task ChallengeAsync(AuthenticationProperties properties) =>
            await _authenticationHandlerHelper.ChallengeAsync(_scheme, _context.Response);

        public async Task ForbidAsync(AuthenticationProperties properties) =>
            await _authenticationHandlerHelper.ForbidAsync(_context.Response);
    }
}