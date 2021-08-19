using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Group.WebApi
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Auth;
    using Microsoft.Azure.Cosmos;
    using Repositories.Connections;
    using Repositories.Users;
    using Services.Azure.Database;
    using Services.Azure.Repositories.Connections;
    using Services.Azure.Repositories.Users;
    using Services.Twilio.Auth;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddAuthentication(options =>
            {
                options.AddScheme<TwilioAuthenticationHandler>("TWILIO", null);
                options.AddScheme<ServerSecretAuthenticationHandler>("SECRET", null);
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TWILIO", x => x
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("TWILIO")
                );

                options.AddPolicy("SECRET", x => x
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("SECRET")
                );
            });
            services.AddControllers();
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo
                        {
                            Title = "Group.WebApi",
                            Version = "v1"
                        });
                });

            services.AddTransient<CosmosClientFactory>();
            services.AddSingleton<CosmosClient>(x => x.GetRequiredService<CosmosClientFactory>().Create());
            services.AddTransient<CosmosContainerProvider>();
            services.AddTransient<UsersRepository>(x => new AzureUsersRepository(
                x.GetRequiredService<CosmosContainerProvider>(),
                x.GetRequiredService<AzureContactRepository>()
            ));
            services.AddTransient<AzureContactRepository>(x => new AzureContactRepository(
                x.GetRequiredService<CosmosContainerProvider>().GetContacts()
            ));
            services.AddTransient<SecretManagerFactory>();
            services.AddSingleton<SecretManager>(x => x.GetRequiredService<SecretManagerFactory>().Create());
            services.AddTransient<ConnectionsRepository>(x => new AzureConnectionsRepository(
                x.GetRequiredService<CosmosContainerProvider>().GetConnections()
            ));
            services.AddSingleton<HttpClient>(_ => new HttpClient(new SocketsHttpHandler
            {
                UseCookies = false
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Group.WebApi v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}