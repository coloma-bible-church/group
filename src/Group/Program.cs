using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Group
{
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables("CBC_GROUP_"))
                .Build()
                .Run();
        }
    }
}