using System;
using System.Net.Http;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZenDeskAutomation.DataLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Services;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Services;
using ZenDeskTicketProcessJob.Utilities;

[assembly: FunctionsStartup(typeof(ZenDeskAutomation.Startup))]
namespace ZenDeskAutomation
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configures the app configuration.
        /// </summary>
        /// <param name="builder">Builder.</param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }


        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="builder">Builder.<see cref="IFunctionsHostBuilder"/></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {

            // Initialize constants
            var configuration = builder.GetContext().Configuration;
            NamesWithTagsConstants.Initialize(configuration);

            builder.Services.AddHttpClient();

            builder.Services.AddApplicationInsightsTelemetry((options) =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            });

            // Configure logging
            builder.Services.AddLogging(loggingBuilder =>
            {
                // Add Application Insights logging
                loggingBuilder.AddApplicationInsights();
            });

            builder.Services.AddSingleton<IDataLayer>((s) =>
            {
                return new DataLayer.Services.DataLayer();
            });

            builder.Services.AddTransient<IZDClientService, ZDClientService>((s) =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                var configuration = s.GetRequiredService<IConfiguration>();
                var schemaTemplateService = s.GetRequiredService<ISchemaTemplateService>();
                return new ZDClientService(httpClientFactory, configuration, schemaTemplateService);
            });

            builder.Services.AddSingleton<ISchemaTemplateService>((s) =>
            {
                return new SchemaTemplateService();
            });

        }
    }

}

