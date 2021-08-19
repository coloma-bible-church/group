using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Group.WebApi
{
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
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