using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using ZendeskTicketProcessingJobCMT;
using ZendeskTicketProcessingJobCMT.DataLayer.Interfaces;
using ZendeskTicketProcessingJobCMT.SchemaTemplateLayer.Interfaces;
using ZendeskTicketProcessingJobCMT.SchemaTemplateLayer.Services;
using ZendeskTicketProcessingJobCMT.Utilities;
using ZendeskTicketProcessingJobCMT.ZendeskLayer.Interfaces;
using ZendeskTicketProcessingJobCMT.ZendeskLayer.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ZendeskTicketProcessingJobCMT
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
            _ = builder.ConfigurationBuilder
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

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddSingleton<IDataLayer>((s) =>
            {
                return new DataLayer.Services.DataLayer();
            });

            builder.Services.AddTransient<IZDClientService, ZDClientService>((s) =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                IConfiguration configuration = s.GetRequiredService<IConfiguration>();
                ISchemaTemplateService schemaTemplateService = s.GetRequiredService<ISchemaTemplateService>();
                return new ZDClientService(httpClientFactory, configuration, schemaTemplateService);
            });

            builder.Services.AddSingleton<ISchemaTemplateService>((s) =>
            {
                return new SchemaTemplateService();
            });

        }
    }

}

