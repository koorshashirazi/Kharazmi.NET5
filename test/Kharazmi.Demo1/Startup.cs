using Kharazmi.Demo1.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kharazmi.BuilderExtensions;
using Kharazmi.Common.Events;
using Kharazmi.Mvc;
using Kharazmi.OpenIdConnect.Extensions;
using Kharazmi.RabbitMq.Extensions;
using Kharazmi.RealTime;
using Kharazmi.Redis.Extensions;
using Kharazmi.Validation;
using MudBlazor.Services;

namespace Kharazmi.Demo1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();


            services.AddCors(op =>
            {
                op.AddPolicy("Framework", cp => cp.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            });

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(
                    "BackgroundJobPolicy",
                    policy => { policy.RequireAuthenticatedUser(); });
            });

            services.AddMudServices();

            var pluginBuilder = services
                .AddCoreConfigurePlugin()
                .AddOpenIdConnectConfigurePlugin()
                .AddRedisConfigurePlugin()
                .AddRabbitMqConfigurePlugin()
                .AddValidationConfigurePlugin()
                .AddMvcConfigurePlugin()
                .AddConfigurePluginsFrom(new[] {typeof(Startup).Assembly});

            pluginBuilder.Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseRouting()
                .UseCors("Framework")
                .UseOpenIdConnectAuthentication();

            app.UseRabbitMqSubscriber()
                .SubscribeTo<RejectedDomainEvent>()
                .SubscribeTo<WizardApp.DomainEvents.V1.UserProcessCreated>((c, e) =>
                    RejectedDomainEvent.Create(e.Message));

            app.UseCorrelationDomainMetadata();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}