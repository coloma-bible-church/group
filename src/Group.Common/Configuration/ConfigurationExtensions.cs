namespace Group.Common.Configuration
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static string GetRequired(this IConfiguration configuration, string name) =>
            configuration[name] ?? throw new Exception($"Cannot find configuration value \"{name}\"");
    }
}