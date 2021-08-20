using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Group.Twilio
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Auth;
    using Common.Auth;
    using Common.Connections;
    using global::Twilio.Clients;
    using Twilio;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddAuthentication(options =>
            {
                options.AddScheme<TwilioAuthenticationHandler>("TWILIO", null);
                options.AddScheme<ConnectionAuthenticationHandler>("CONNECTION", null);
                options.AddScheme<SecretHeaderAuthenticationHandler>("SECRET", null);
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TWILIO", x => x
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("TWILIO")
                );
                options.AddPolicy("CONNECTION", x => x
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("CONNECTION")
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
                            Title = "Group.Twilio",
                            Version = "v1"
                        });
                });

            services.AddSingleton<HttpClient>(_ => new HttpClient(new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false
            }));
            services.AddTransient<TwilioRestClientFactory>();
            services.AddTransient<ITwilioRestClient>(x => x.GetRequiredService<TwilioRestClientFactory>().Create());
            services.AddTransient<AuthenticationHandlerHelper>();
            services.AddTransient<ConnectionFactory>();
            services.AddSingleton<Connection>(x => x.GetRequiredService<ConnectionFactory>().Create());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Group.Twilio v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}