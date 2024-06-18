using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using ZenDeskTicketProcessJob;
using ZenDeskTicketProcessJob.DataLayer.Interfaces;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Services;
using ZenDeskTicketProcessJob.Utilities;
using ZenDeskTicketProcessJob.ZenDeskLayer.Interfaces;
using ZenDeskTicketProcessJob.ZenDeskLayer.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ZenDeskTicketProcessJob
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
            IConfiguration configuration = builder.GetContext().Configuration;
            NamesWithTagsConstants.Initialize(configuration);

            _ = builder.Services.AddHttpClient();

            _ = builder.Services.AddApplicationInsightsTelemetry();

            _ = builder.Services.AddSingleton<IDataLayer>((s) =>
            {
                return new DataLayer.Services.DataLayer();
            });

            _ = builder.Services.AddTransient<IZDClientService, ZDClientService>((s) =>
            {
                IHttpClientFactory httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                IConfiguration configuration = s.GetRequiredService<IConfiguration>();
                ISchemaTemplateService schemaTemplateService = s.GetRequiredService<ISchemaTemplateService>();
                return new ZDClientService(httpClientFactory, configuration, schemaTemplateService);
            });

            _ = builder.Services.AddSingleton<ISchemaTemplateService>((s) =>
            {
                return new SchemaTemplateService();
            });

        }
    }

}

