namespace Group.Auth
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;

    public class MagicHeaderHandler : IAuthenticationHandler
    {
        public const string Scheme = "MAGIC";

        readonly IOptionsMonitor<MagicHeaderHandlerOptions> _optionsMonitor;
        bool _hasMagic;
        readonly string _magic;
        MagicHeaderHandlerOptions _options = null!;
        HttpContext _context = null!;

        public MagicHeaderHandler(
            IConfiguration configuration,
            IOptionsMonitor<MagicHeaderHandlerOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
            _magic = configuration.GetValue<string>(Scheme);
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            await Task.CompletedTask;
            _context = context;
            if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationValues))
            {
                _hasMagic = false;
                return;
            }
            _hasMagic = authorizationValues.Any(x => x.Contains(_magic));
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            await Task.CompletedTask;
            _options = _optionsMonitor.Get(Scheme);
            return _hasMagic
                ? AuthenticateResult.Success(new AuthenticationTicket(_options.Principal, Scheme))
                : AuthenticateResult.NoResult();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }
    }
}