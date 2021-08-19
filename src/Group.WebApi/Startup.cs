using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Group.WebApi
{
    using System.Diagnostics.CodeAnalysis;
    using Auth;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Azure.Cosmos;
    using Repositories.Users;
    using Services.Azure.Database;
    using Services.Azure.Repositories;
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
                options.AddPolicy("TWILIO", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("TWILIO")
                    .Build());

                options.AddPolicy("SECRET", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("SECRET")
                    .Build());
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
            services.AddTransient<AzureUsersRepository>(
                x => new AzureUsersRepository(
                    x.GetRequiredService<CosmosContainerProvider>(),
                    x.GetRequiredService<AzureContactRepository>()
                )
            );
            services.AddTransient(
                typeof(UsersRepository),
                x => x.GetRequiredService<AzureUsersRepository>()
            );
            services.AddTransient<AzureContactRepository>(x =>
            {
                var provider = x.GetRequiredService<CosmosContainerProvider>();
                return new AzureContactRepository(
                    provider.GetContacts()
                );
            });
            services.AddTransient<SecretManagerFactory>();
            services.AddSingleton<SecretManager>(x => x.GetRequiredService<SecretManagerFactory>().Create());
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