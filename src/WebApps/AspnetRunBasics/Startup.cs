using AspnetRunBasics.Services;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;

namespace AspnetRunBasics
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var gtwAddress = _configuration["ApiSettings:GatewayAddress"];

            services.AddTransient<LoggingDelegatingHandler>();
            services.AddHttpClient<ICatalogService, CatalogService>(c =>
                c.BaseAddress = new Uri(gtwAddress))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(gtwAddress))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(gtwAddress))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();

            services.AddRazorPages();

            services.AddHealthChecks()
                    .AddUrlGroup(uri: new Uri($"{_configuration["ApiSettings:GatewayAddress"]}"),
                                 name: "Gateway API",
                                 failureStatus: HealthStatus.Degraded);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}
