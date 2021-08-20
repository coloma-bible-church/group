namespace Group.Common.Auth
{
    using System.Threading.Tasks;
    using Configuration;
    using Connections;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class ConnectionAuthenticationHandler : IAuthenticationHandler
    {
        readonly IConfiguration _configuration;
        readonly AuthenticationHandlerHelper _authenticationHandlerHelper;
        string _connectionSecret = null!;
        HttpContext _context = null!;
        AuthenticationScheme _scheme = null!;

        public ConnectionAuthenticationHandler(
            IConfiguration configuration,
            AuthenticationHandlerHelper authenticationHandlerHelper)
        {
            _configuration = configuration;
            _authenticationHandlerHelper = authenticationHandlerHelper;
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _context = context;
            _scheme = scheme;
            _connectionSecret = _configuration.GetRequired("CONNECTION_SECRET");
            await Task.CompletedTask;
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            var connectionSecret = _context.Request.Headers[ConnectionHeaders.ConnectionSecretHeaderName].ToString();
            if (string.IsNullOrWhiteSpace(connectionSecret))
                return AuthenticateResult.NoResult();
            if (!SecureCompare.Compare(connectionSecret, _connectionSecret))
                return AuthenticateResult.Fail("Invalid " + ConnectionHeaders.ConnectionSecretHeaderName + " header");
            await Task.CompletedTask;
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