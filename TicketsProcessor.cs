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
        public TicketsProcessor(IDataLayer dataLayer, IConfiguration configuration,
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
            try
            {
                _logger.LogInformation("*********Azure function execution started**********");

                Task.Run(async () =>
                {
                    _logger.LogInformation("Task started");

                    string CRMConnectionString = _configuration["DataBase:CRMConnectionString"];

                    var sqlParams = new Dictionary<string, object>
                    {
                        {"@date", _configuration["CurrentDate"]},
                        {"@count", _configuration["Count"] }
                    };

                    var caseManagementTickets = await _dataLayer.ExecuteReader<CaseTickets>(SQLConstants.GetMemberCaseTicketsForZenDesk, sqlParams, CRMConnectionString, _logger);

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    _logger.LogInformation($"Received case management tickets with count: {caseManagementTickets?.Count}");

                    foreach (var caseManagementTicket in caseManagementTickets)
                    {
                        if (!string.IsNullOrWhiteSpace(caseManagementTicket?.ZendeskTicket) && caseManagementTicket?.ZendeskTicket?.Length > 0)
                        {
                            _logger.LogInformation($"Started updating ticket via = zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.UpdateTicketInZenDeskAsync(caseManagementTicket);

                            _logger.LogInformation($"Successfully updated zendesk ticket for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference);

                        }
                        else
                        {
                            _logger.LogInformation($"Started creating ticket via = zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.CreateTicketInZenDeskAsync(caseManagementTicket);

                            _logger.LogInformation($"Successfully created zendesk ticket for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference);
                        }
                    }

                    _logger.LogInformation("Task ended");

                });

                _logger.LogInformation("*********Azure function execution ended*********");

                return new OkObjectResult("Azure function executed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Updates the zendesk ticket reference and is processed status.
        /// </summary>
        /// <param name="_logger">Logger.</param>
        /// <param name="brConnectionString">BR connection string.</param>
        /// <param name="caseManagementTicket">Case management ticket.</param>
        /// <param name="ticketNumberReference">Ticket number reference.</param>
        private async Task UpdatesZendeskTicketReferenceAndIsProcessedStatus(ILogger _logger, string brConnectionString, CaseTickets caseManagementTicket, long ticketNumberReference)
        {
            var result = await _dataLayer.ExecuteNonQuery(SQLConstants.UpdateZenDeskReferenceForMemberCaseTickets, caseManagementTicket.CaseTicketID, ticketNumberReference, brConnectionString, _logger);

            if (result == 1)
            {
                _logger.LogInformation($"Updated the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
            else
            {
                _logger.LogError($"Failed to update the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
        }

        #endregion
    }
}
