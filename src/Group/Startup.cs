using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Group
{
    using System;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using Auth;
    using MessageSinks.Azure;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions<MagicHeaderHandlerOptions>(MagicHeaderHandler.Scheme)
                .Configure(options =>
                    options.Principal = new GenericPrincipal(
                        new ClaimsIdentity(Array.Empty<Claim>()),
                        new []
                        {
                            SmsHelpers.SmsContributorRole
                        }
                    )
                );
            services.Configure<SmsBroadcastMessageSinkOptions>(Configuration.GetSection(SmsBroadcastMessageSinkOptions.SectionName));
            services.AddControllers();
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo
                        {
                            Title = "Group",
                            Version = "v1"
                        });
                });
            services.AddAuthentication(options =>
            {
                options.AddScheme<MagicHeaderHandler>(MagicHeaderHandler.Scheme, null);
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(SmsHelpers.CanSendSmsPolicy, builder =>
                {
                    builder.AddAuthenticationSchemes(MagicHeaderHandler.Scheme);
                    builder.RequireRole(SmsHelpers.SmsContributorRole);
                });
            });
            foreach (var type in typeof(RegisterAttribute).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<RegisterAttribute>() is not {} registerAttribute)
                    continue;
                services.AddTransient(registerAttribute.ServiceType, type);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Group v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}