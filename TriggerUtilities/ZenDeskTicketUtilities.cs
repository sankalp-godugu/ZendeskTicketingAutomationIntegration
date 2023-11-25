using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZenDeskAutomation.DataLayer.Interfaces;
using ZenDeskAutomation.Utilities;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskTicketProcessJob.TriggerUtilities
{

    /// <summary>
    /// Zendesk utilities.
    /// </summary>
    public static class ZenDeskTicketUtilities
    {

        /// <summary>
        /// Processes Zendesk tickets, performing operations such as logging, configuration retrieval,
        /// data layer interaction, and Zendesk API service calls.
        /// </summary>
        /// <param name="_logger">An instance of the <see cref="ILogger"/> interface for logging.</param>
        /// <param name="_configuration">An instance of the <see cref="IConfiguration"/> interface for accessing configuration settings.</param>
        /// <param name="_dataLayer">An instance of the <see cref="IDataLayer"/> interface or class for interacting with the data layer.</param>
        /// <param name="_zdClientService">An instance of the <see cref="IZDClientService"/> interface or class for Zendesk API service calls.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the Zendesk ticket processing.</returns>
        public static IActionResult ProcessZenDeskTickets(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IZDClientService _zdClientService)
        {
            try
            {
                _logger.LogInformation("*********Azure function execution started**********");

                Task.Run(async () =>
                {
                    _logger.LogInformation("Task started");

                    // CRM connection string.
                    string CRMConnectionString = _configuration["DataBase:CRMConnectionString"];

                    // SQL parameters.
                    var sqlParams = new Dictionary<string, object>
                    {
                        {"@date", _configuration["CurrentDate"]},
                        {"@count", _configuration["Count"] }
                    };

                    _logger.LogInformation("Started fetching the case tickets for all members");

                    var caseManagementTickets = await _dataLayer.ExecuteReader<CaseTickets>(SQLConstants.GetAllCaseTicketsForAllMembers, sqlParams, CRMConnectionString, _logger);

                    _logger.LogInformation($"Ended fetching the case tickets with count: {caseManagementTickets?.Count}");

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    foreach (var caseManagementTicket in caseManagementTickets)
                    {
                        if (!string.IsNullOrWhiteSpace(caseManagementTicket?.ZendeskTicket) && Convert.ToInt64(caseManagementTicket?.ZendeskTicket) > 0)
                        {
                            _logger.LogInformation($"Started updating ticket via zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService?.UpdateTicketInZenDeskAsync(caseManagementTicket, _logger);

                            _logger.LogInformation($"Successfully updated zendesk ticket for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference, _dataLayer);

                        }
                        else
                        {
                            _logger.LogInformation($"Started creating ticket via = zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.CreateTicketInZenDeskAsync(caseManagementTicket, _logger);

                            _logger.LogInformation($"Successfully created zendesk ticket for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference, _dataLayer);
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
        private static async Task UpdatesZendeskTicketReferenceAndIsProcessedStatus(ILogger _logger, string brConnectionString, CaseTickets caseManagementTicket, long ticketNumberReference, IDataLayer _dataLayer)
        {
            _logger.LogInformation($"Updating zendesk ticket details with caseticket id :{caseManagementTicket?.ZendeskTicket}");

            var result = await _dataLayer.ExecuteNonQuery(SQLConstants.UpdateZenDeskReferenceForMemberCaseTickets, caseManagementTicket.CaseTicketID, ticketNumberReference, brConnectionString, _logger);

            if (result == 1)
            {
                _logger?.LogInformation($"Updated the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
            else
            {
                _logger?.LogInformation($"Failed to update the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
        }
    }
}
