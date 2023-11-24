using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ZenDeskAutomation.DataLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskTicketProcessJob.TriggerUtilities;

namespace ZenDeskAutomation
{
    /// <summary>
    /// Azure function for processing all the tickets of card and reimbursement requests of date greater than specified in keyvault and a grace period of 30 minutes.
    /// </summary>
    public class HttpFunction
    {
        #region Private ReadOnly Fields
        private readonly ILogger<HttpFunction> _logger;
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;
        #endregion

        #region Constructor

        /// <summary>
        /// Tickets processor
        /// </summary>
        /// <param name="log">Logger.<see cref="ILogger"/></param>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        public HttpFunction(IDataLayer dataLayer, IConfiguration configuration,
            IZDClientService zdClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }

        #endregion

        #region Methods

        [FunctionName("TicketsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger _logger)
        {
            return ZenDeskTicketUtilities.ProcessZenDeskTickets(_logger, _configuration, _dataLayer, _zdClientService);
        }
        #endregion
    }

    public class TimerFunction
    {
        private readonly ILogger<TimerFunction> _logger;
        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IZDClientService _zdClientService;

        public TimerFunction(ILogger<TimerFunction> logger, IDataLayer dataLayer, IConfiguration configuration, IZDClientService zdClientService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }

        [FunctionName("TimerFunction")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            ZenDeskTicketUtilities.ProcessZenDeskTickets(_logger, _configuration, _dataLayer, _zdClientService);
        }
    }
}
