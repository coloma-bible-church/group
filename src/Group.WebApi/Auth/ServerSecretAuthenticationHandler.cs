namespace Group.WebApi.Auth
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ServerSecretAuthenticationHandler : IAuthenticationHandler
    {
        readonly SecretManager _secretManager;
        AuthenticationScheme _scheme = null!;
        HttpContext _context = null!;

        public ServerSecretAuthenticationHandler(
            SecretManager secretManager)
        {
            _secretManager = secretManager;
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
                    return AuthenticateResult.Success(CreateTicket());
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
                    && FromBase64String(secretValue) is {} secretBytes
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

                    return AuthenticateResult.Success(CreateTicket());
                }

                return AuthenticateResult.NoResult();
            }
            finally
            {
                request.Body.Position = 0;
            }
        }

        AuthenticationTicket CreateTicket() =>
            new(
                new GenericPrincipal(
                    new GenericIdentity(Guid.NewGuid().ToString(), _scheme.Name),
                    null
                ),
                _scheme.Name
            );

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            await Task.CompletedTask;
            _context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            _context.Response.Headers["WWW-Authenticate"] = _scheme.Name;
        }

        byte[]? GetCookieBytes(string cookieName)
        {
            if (!_context.Request.Cookies.TryGetValue(cookieName, out var cookieValue) || cookieValue is null)
                return null;
            return FromBase64String(cookieValue);
        }

        static byte[]? FromBase64String(string value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                return null;
            }
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            await Task.CompletedTask;
            _context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
        }
    }
}