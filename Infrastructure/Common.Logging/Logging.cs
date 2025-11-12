using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging;

public class Logging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (context, config) =>
    {
        var env = context.HostingEnvironment;
        config.MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("EnvironmentName", env.EnvironmentName)
            .Enrich.WithProperty("ApplicationName", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .WriteTo.Console();

        if (context.HostingEnvironment.IsDevelopment())
        {
            config.MinimumLevel.Override("Catalog", LogEventLevel.Debug);
            config.MinimumLevel.Override("Basket", LogEventLevel.Debug);
            config.MinimumLevel.Override("Discount", LogEventLevel.Debug);
            config.MinimumLevel.Override("Ordering", LogEventLevel.Debug);
            config.MinimumLevel.Override("Payment", LogEventLevel.Debug);
            config.MinimumLevel.Override("Identity", LogEventLevel.Debug);
        }
        
        // Elastic search sink configuration (uncomment and configure if needed)
        var elasticUri = context.Configuration["elasticSearchSettings:Uri"];
        if (!string.IsNullOrEmpty(elasticUri))
        {
            config.WriteTo.Elasticsearch(
            new ElasticsearchSinkOptions(new Uri(elasticUri)) 
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                IndexFormat = "ecommerce-log-{0:yyyy.MM.dd}",
                MinimumLogEventLevel = LogEventLevel.Debug
            });
        }
    };
    
}