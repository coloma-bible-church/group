using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Group.Hub
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .ConfigureLogging(builder =>
                {
                    builder.AddApplicationInsights();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddUserSecrets<Program>();
                    builder.AddEnvironmentVariables();
                })
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}