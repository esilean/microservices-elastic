using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Common.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
            (ctx, config) =>
            {
                var elasticUri = ctx.Configuration["ElasticConfig:Uri"];
                var indexFormat = $"applogs-{ctx.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-")}-{ctx.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}";
                config
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
                        {
                            IndexFormat = indexFormat,
                            AutoRegisterTemplate = true,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1
                        })
                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
                    .ReadFrom.Configuration(ctx.Configuration);
            };
    }
}
