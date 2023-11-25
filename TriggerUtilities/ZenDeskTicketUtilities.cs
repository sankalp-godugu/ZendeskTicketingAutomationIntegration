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
    public static class ZenDeskTicketUtilities
    {
        public static IActionResult ProcessZenDeskTickets(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IZDClientService _zdClientService)
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

                    var caseManagementTickets = await _dataLayer.ExecuteReader<CaseTickets>(SQLConstants.GetAllCaseTicketsForAllMembers, sqlParams, CRMConnectionString, _logger);

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    _logger.LogInformation($"Received case management tickets with count: {caseManagementTickets?.Count}");

                    foreach (var caseManagementTicket in caseManagementTickets)
                    {
                        if (!string.IsNullOrWhiteSpace(caseManagementTicket?.ZendeskTicket) && caseManagementTicket?.ZendeskTicket?.Length > 0)
                        {
                            _logger.LogInformation($"Started updating ticket via = zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.UpdateTicketInZenDeskAsync(caseManagementTicket);

                            _logger.LogInformation($"Successfully updated zendesk ticket for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference, _dataLayer);

                        }
                        else
                        {
                            _logger.LogInformation($"Started creating ticket via = zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.CreateTicketInZenDeskAsync(caseManagementTicket);

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
    }
}
