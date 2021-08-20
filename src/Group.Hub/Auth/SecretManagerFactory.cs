namespace Group.Hub.Auth
{
    using System;
    using System.Security.Cryptography;
    using Common.Configuration;
    using Microsoft.Extensions.Configuration;

    public class SecretManagerFactory
    {
        readonly IConfiguration _configuration;

        public SecretManagerFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SecretManager Create()
        {
            var secret = Convert.FromBase64String(_configuration.GetRequired("SERVER_SECRET"));
            Func<HMAC> genHmac = secret.Length switch
            {
                128 / 8 => () => new HMACMD5(secret),
                160 / 8 => () => new HMACSHA1(secret),
                256 / 8 => () => new HMACSHA256(secret),
                384 / 8 => () => new HMACSHA384(secret),
                512 / 8 => () => new HMACSHA512(secret),
                _ => throw new Exception("Unexpected server secret byte length")
            };
            return new SecretManager(genHmac);
        }
    }
}