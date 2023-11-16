using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
using ZenDeskAutomation.Utilities;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskAutomation.ZenDeskLayer.Services;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskAutomation
{
    /// <summary>
    /// Azure function for processing all the tickets of card and reimbursement requests of date greater than specified in keyvault and a grace period of 30 minutes.
    /// </summary>
    public class TicketsProcessor
    {
        #region Private ReadOnly Fields
        private readonly ILogger<TicketsProcessor> _logger;
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
        public TicketsProcessor(ILogger<TicketsProcessor> log, IDataLayer dataLayer, IConfiguration configuration,
            IZDClientService zdClientService)
        {
            _logger = log ?? throw new ArgumentNullException(nameof(log));
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _zdClientService = zdClientService ?? throw new ArgumentNullException(nameof(zdClientService));
        }

        #endregion

        #region Methods

        [FunctionName("TicketsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("*********Azure function execution started**********");

                Task.Run(async () =>
                {
                    _logger.LogInformation("Task started");

                    string CRMConnectionString = Environment.GetEnvironmentVariable("DataBase:CRMConnectionString");

                    var sqlParams = new Dictionary<string, object>
                    {
                        {"@date", "2021-12-31"},
                    };

                    var caseManagementTickets = await _dataLayer.ExecuteReader<CaseTickets>(SQLConstants.GetMemberCaseTicketsForZenDesk, sqlParams, CRMConnectionString, _logger);

                    foreach(var caseManagementTicket in caseManagementTickets)
                    {
                        await _zdClientService.CreateTicketInZenDeskAsync(caseManagementTicket);
                    }

                    _logger.LogInformation("Task ended");

                });

                _logger.LogInformation("*********Azure function execution ended*********");

                return new OkObjectResult("Azure function executed successfully");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        #endregion
    }
}
