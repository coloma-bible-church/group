namespace Group.Auth
{
    using System;
    using System.Security.Claims;
    using System.Security.Principal;

    public class MagicHeaderHandlerOptions
    {
        static readonly GenericPrincipal RandomPrincipal = new(new GenericIdentity(Guid.NewGuid().ToString()), Array.Empty<string>());

        public ClaimsPrincipal Principal { get; set; } = RandomPrincipal;
    }
}