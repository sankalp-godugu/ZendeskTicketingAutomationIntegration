using System;
using System.Net.Http;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenDeskAutomation.DataLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Services;

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
            //var config = builder.ConfigurationBuilder
            //    .SetBasePath(Environment.CurrentDirectory)
            //    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            //    .AddEnvironmentVariables()
            //    .Build();

            //var keyVaultUri = config["Values:KeyVaultUri"];

            //builder.ConfigurationBuilder.AddAzureKeyVault(
            //    new Uri(keyVaultUri),
            //    new DefaultAzureCredential());
        }


        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="builder">Builder.<see cref="IFunctionsHostBuilder"/></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IDataLayer>((s) =>
            {
                return new DataLayer.Services.DataLayer();
            });

            builder.Services.AddTransient<IZDClientService, ZDClientService>((s) =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                var configuration = s.GetRequiredService<IConfiguration>();
                return new ZDClientService(httpClientFactory, configuration);
            });

        }
    }

}

