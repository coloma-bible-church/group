namespace Group.Hub.Parsing
{
    using System;

    public static class Base64Helpers
    {
        public static byte[]? TryParse(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }
    }
}