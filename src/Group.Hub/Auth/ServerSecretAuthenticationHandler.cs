namespace Group.Hub.Auth
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Auth;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Parsing;

    public class ServerSecretAuthenticationHandler : IAuthenticationHandler
    {
        readonly SecretManager _secretManager;
        readonly AuthenticationHandlerHelper _authenticationHandlerHelper;
        AuthenticationScheme _scheme = null!;
        HttpContext _context = null!;

        public ServerSecretAuthenticationHandler(
            SecretManager secretManager,
            AuthenticationHandlerHelper authenticationHandlerHelper)
        {
            _secretManager = secretManager;
            _authenticationHandlerHelper = authenticationHandlerHelper;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _scheme = scheme;
            _context = context;
            return Task.CompletedTask;
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            await Task.CompletedTask;
            var saltCookieName = $"{_scheme.Name}-salt";
            var hashCookieName = $"{_scheme.Name}-hash";
            if (GetCookieBytes(saltCookieName) is {} salt && GetCookieBytes(hashCookieName) is {} hash)
            {
                if (_secretManager.Check(salt, hash))
                {
                    return AuthenticateResult.Success(_authenticationHandlerHelper.CreateTicket(_scheme));
                }
            }
            _context.Response.Cookies.Delete(saltCookieName);
            _context.Response.Cookies.Delete(hashCookieName);

            var request = _context.Request;
            request.EnableBuffering();
            try
            {
                if (request.HasFormContentType
                    && request.Form.TryGetValue(_scheme.Name, out var secretValues)
                    && secretValues.FirstOrDefault() is {} secretValue
                    && Base64Helpers.TryParse(secretValue) is {} secretBytes
                    && _secretManager.CheckRaw(secretBytes))
                {
                    (salt, hash) = _secretManager.Create();
                    _context.Response.Cookies.Append(
                        saltCookieName,
                        Convert.ToBase64String(salt),
                        new CookieOptions
                        {
                            Secure = true
                        }
                    );
                    _context.Response.Cookies.Append(
                        hashCookieName,
                        Convert.ToBase64String(hash),
                        new CookieOptions
                        {
                            Secure = true
                        }
                    );

                    return AuthenticateResult.Success(_authenticationHandlerHelper.CreateTicket(_scheme));
                }

                return AuthenticateResult.NoResult();
            }
            finally
            {
                request.Body.Position = 0;
            }
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            await _authenticationHandlerHelper.ChallengeAsync(_scheme, _context.Response);
        }

        byte[]? GetCookieBytes(string cookieName)
        {
            if (!_context.Request.Cookies.TryGetValue(cookieName, out var cookieValue) || cookieValue is null)
                return null;
            return Base64Helpers.TryParse(cookieValue);
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            await _authenticationHandlerHelper.ForbidAsync(_context.Response);
        }
    }
}