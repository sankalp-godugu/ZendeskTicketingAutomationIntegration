using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using ZenDeskTicketProcessJob.DataLayer.Interfaces;
using ZenDeskTicketProcessJob.TriggerUtilities;
using ZenDeskTicketProcessJob.ZenDeskLayer.Interfaces;

namespace ZenDeskTicketProcessJob
{
    /// <summary>
    /// Azure http function for processing all the tickets of card and grievance requests of date greater than specified in keyvault.
    /// </summary>
    public class CMTAzureHttpFunction
    {
        #region Private ReadOnly Fields
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;
        #endregion

        #region Constructor

        /// <summary>
        /// Http function that will be invoked via endpoint.
        /// </summary>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        /// <param name="zdClientService">Zendesk client service.<see cref="IZDClientService"/></param>
        public CMTAzureHttpFunction(IDataLayer dataLayer, IConfiguration configuration, IZDClientService zdClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Case management tickets processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("CMTTicketsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger _logger)
        {
            return ZenDeskTicketUtilities.ProcessCMTZenDeskTickets(_logger, _configuration, _dataLayer, _zdClientService);
        }

        #endregion
    }

    /// <summary>
    /// Azure http function for processing all the tickets of OTC refund and reship orders.
    /// </summary>
    public class AdminAzureHttpFunction
    {

        #region Private ReadOnly Fields
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;
        #endregion

        #region Constructor

        /// <summary>
        /// Http function that will be invoked via endpoint.
        /// </summary>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        /// <param name="zdClientService">Zendesk client service.<see cref="IZDClientService"/></param>
        public AdminAzureHttpFunction(IDataLayer dataLayer, IConfiguration configuration, IZDClientService zdClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// OTC refund and reship orders processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("OTCTicketsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger _logger)
        {
            return ZenDeskTicketUtilities.ProcessAdminZenDeskTickets(_logger, _configuration, _dataLayer, _zdClientService);
        }

        #endregion
    }

    /// <summary>
    /// An azure function that runs on an interval for every one minute.
    /// </summary>
    public class CMTAzureTimerFunction
    {
        #region Private Readonly Fields
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;
        #endregion

        #region Constructor.

        /// <summary>
        /// Http function that will be invoked via endpoint.
        /// </summary>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        /// <param name="zdClientService">Zendesk client service.<see cref="IZDClientService"/></param>
        public CMTAzureTimerFunction(IDataLayer dataLayer, IConfiguration configuration, IZDClientService zdClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }
        #endregion

        /// <summary>
        /// Runs the azure function for every one minute.
        /// </summary>
        /// <param name="myTimer">My timer.</param>
        [FunctionName("CMTAzureTimerFunction")]
        public void Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger logger)
        {
            if (_configuration.GetValue("IsCMTJobEnabled", true))
            {
                _ = ZenDeskTicketUtilities.ProcessCMTZenDeskTickets(logger, _configuration, _dataLayer, _zdClientService);
            }
            else
            {
                logger.LogInformation("Processing is disabled. Skipping execution.");
            }
        }
    }

    /// <summary>
    /// An azure function that runs on an interval for every one minute.
    /// </summary>
    public class AdminAzureTimerFunction
    {
        #region Private Readonly Fields
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;
        #endregion

        #region Constructor.

        /// <summary>
        /// Http function that will be invoked via endpoint.
        /// </summary>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        /// <param name="zdClientService">Zendesk client service.<see cref="IZDClientService"/></param>
        public AdminAzureTimerFunction(IDataLayer dataLayer, IConfiguration configuration, IZDClientService zdClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }
        #endregion

        /// <summary>
        /// Runs the azure function for every one minute.
        /// </summary>
        /// <param name="myTimer">My timer.</param>
        [FunctionName("AdminAzureTimerFunction")]
        public void Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger logger)
        {
            if (_configuration.GetValue("IsAdminJobEnabled", true))
            {
                _ = ZenDeskTicketUtilities.ProcessAdminZenDeskTickets(logger, _configuration, _dataLayer, _zdClientService);
            }
            else
            {
                logger.LogInformation("Processing is disabled. Skipping execution.");
            }
        }
    }
}
