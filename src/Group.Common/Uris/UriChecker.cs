namespace Group.Common.Uris
{
    using System;

    public static class UriChecker
    {
        public static bool IsValidAndSecure(string? raw)
        {
            if (raw is null)
                return false;
            if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
                return false;
            return uri.Scheme == "https";
        }
    }
}