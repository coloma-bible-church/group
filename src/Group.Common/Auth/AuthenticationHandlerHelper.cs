namespace Group.Common.Auth
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class AuthenticationHandlerHelper
    {
        public Task ChallengeAsync(AuthenticationScheme scheme, HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.Unauthorized;
            response.Headers["WWW-Authenticate"] = scheme.Name;
            return Task.CompletedTask;
        }

        public AuthenticationTicket CreateTicket(AuthenticationScheme scheme) => new(
            new GenericPrincipal(
                new GenericIdentity(Guid.NewGuid().ToString(), scheme.Name),
                null
            ),
            scheme.Name
        );

        public Task ForbidAsync(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }
    }
}