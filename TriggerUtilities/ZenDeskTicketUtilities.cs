using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static IActionResult ProcessCMTZenDeskTickets(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IZDClientService _zdClientService)
        {
            try
            {
                Task.Run(async () =>
                {
                    _logger?.LogInformation("********* Case Management Ticket(CMT) => ZenDesk Execution Started **********");

                    // CRM connection string.
                    string CRMConnectionString = _configuration["DataBase:CRMConnectionString"];

                    // SQL parameters.
                    var sqlParams = new Dictionary<string, object>
                    {
                        {"@date", _configuration["CurrentDate"]},
                        {"@count", _configuration["Count"] }
                    };

                    _logger?.LogInformation("Started fetching the case tickets for all members");

                    var caseManagementTickets = await _dataLayer.ExecuteReader<CaseTickets>(SQLConstants.GetAllCaseTicketsForAllMembers, sqlParams, CRMConnectionString, _logger);

                    _logger?.LogInformation($"Ended fetching the case tickets with count: {caseManagementTickets?.Count}");

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    foreach (var caseManagementTicket in caseManagementTickets)
                    {
                        if (!string.IsNullOrWhiteSpace(caseManagementTicket?.ZendeskTicket) && Convert.ToInt64(caseManagementTicket?.ZendeskTicket) > 0)
                        {
                            _logger?.LogInformation($"Started updating ticket via zendesk API for the case management ticket id: {caseManagementTicket?.CaseTicketID} for NHMemberID {caseManagementTicket?.NHMemberID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService?.UpdateCMTTicketInZenDeskAsync(caseManagementTicket, _logger);

                            _logger?.LogInformation($"Successfully updated zendesk ticket id {ticketNumberReference} for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesCMTZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference, _dataLayer);

                        }
                        else
                        {
                            _logger?.LogInformation($"Started creating ticket via zendesk API for the case management ticket id: {caseManagementTicket.CaseTicketID}  for NHMemberID {caseManagementTicket?.NHMemberID} with details {caseManagementTicket}");

                            var ticketNumberReference = await _zdClientService.CreateCMTTicketInZenDeskAsync(caseManagementTicket, _logger);

                            _logger?.LogInformation($"Successfully created zendesk ticket id {ticketNumberReference} for the case management ticket id: {caseManagementTicket.CaseTicketID} with details {caseManagementTicket}");

                            await UpdatesCMTZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, ticketNumberReference, _dataLayer);
                        }
                    }

                    _logger?.LogInformation("********* Case Management Ticket(CMT) => ZenDesk Execution Ended *********");

                });

                return new OkObjectResult("Task of CMT to Zendesk has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Processes admin Zendesk tickets, performing operations such as logging, configuration retrieval, data layer interaction, and Zendesk API service calls.
        /// </summary>
        /// <param name="_logger">An instance of the <see cref="ILogger"/> interface for logging.</param>
        /// <param name="_configuration">An instance of the <see cref="IConfiguration"/> interface for accessing configuration settings.</param>
        /// <param name="_dataLayer">An instance of the <see cref="IDataLayer"/> interface or class for interacting with the data layer.</param>
        /// <param name="_zdClientService">An instance of the <see cref="IZDClientService"/> interface or class for Zendesk API service calls.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the Zendesk ticket processing.</returns>
        public static IActionResult ProcessAdminZenDeskTickets(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IZDClientService _zdClientService)
        {
            try
            {
                Task.Run(async () =>
                {
                    _logger?.LogInformation("********* Admin Portal => ZenDesk Execution Started **********");

                    // CRM connection string.
                    string CRMConnectionString = _configuration["DataBase:CRMConnectionString"];

                    // SQL parameters.
                    var sqlParams = new Dictionary<string, object>
                    {
                        {"@date", _configuration["AdminCurrentDate"]},
                        {"@count", _configuration["AdminCount"] }
                    };

                    _logger?.LogInformation("Started fetching the refund and reship over the counter orders");

                    var orderChangeRequestIds = await _dataLayer.ExecuteReader<OrderChangeRequest>(SQLConstants.GetOrderChangeRequestsForZenDeskIntegration, sqlParams, CRMConnectionString, _logger);

                    _logger?.LogInformation($"Ended fetching the refund and reship over the counter orders with count: {orderChangeRequestIds?.Count}");

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    foreach (var orderChangeRequestId in orderChangeRequestIds)
                    {

                        var keyValuePairs = new Dictionary<string, object>
                        {
                            {"@OrderChangeRequestId", orderChangeRequestId.OrderChangeRequestId}
                        };

                        var orderChangeRequest = (await _dataLayer.ExecuteReader<Order>(SQLConstants.GetOrderDetailsForZenDeskIntegrationByChangeRequestId, keyValuePairs, CRMConnectionString, _logger)).FirstOrDefault();

                        if (orderChangeRequest != null)
                        {
                            if (!string.IsNullOrWhiteSpace(orderChangeRequest?.TicketId) && Convert.ToInt64(orderChangeRequest?.TicketId) > 0)
                            {
                                _logger?.LogInformation($"Started updating ticket via zendesk API for the admin ticket id: {orderChangeRequest.TicketId} for NHMemberID {orderChangeRequest?.NHMemberId} with details {orderChangeRequest}");

                                var ticketNumberReference = await _zdClientService?.UpdateAdminTicketInZenDeskAsync(orderChangeRequest, _logger);

                                _logger?.LogInformation($"Successfully updated zendesk ticket id {ticketNumberReference} for the order change request id: {orderChangeRequest.OrderChangeRequestId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, ticketNumberReference, _dataLayer);

                            }
                            else
                            {
                                _logger?.LogInformation($"Started creating ticket via zendesk API for the order change request id: {orderChangeRequest.OrderChangeRequestId}  for NHMemberID {orderChangeRequest?.NHMemberId} with details {orderChangeRequest}");

                                var ticketNumberReference = await _zdClientService.CreateAdminTicketInZenDeskAsync(orderChangeRequest, _logger);

                                _logger?.LogInformation($"Successfully created zendesk ticket id {ticketNumberReference} for the order change request id: {orderChangeRequest.OrderChangeRequestId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, ticketNumberReference, _dataLayer);
                            }
                        }
                    }

                    _logger?.LogInformation("********* Case Management Ticket(CMT) => ZenDesk Execution Ended *********");

                });

                return new OkObjectResult("Task of OTC reship or refund orders to Zendesk has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed with an exception with message: {ex.Message}");
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
        private static async Task UpdatesCMTZendeskTicketReferenceAndIsProcessedStatus(ILogger _logger, string brConnectionString, CaseTickets caseManagementTicket, long ticketNumberReference, IDataLayer _dataLayer)
        {
            _logger?.LogInformation($"Updating zendesk ticket details with caseticket id :{caseManagementTicket?.ZendeskTicket}");

            var result = await _dataLayer.ExecuteNonQueryForCaseManagement(SQLConstants.UpdateZenDeskReferenceForMemberCaseTickets, caseManagementTicket.CaseTicketID, ticketNumberReference, brConnectionString, _logger);

            if (result == 1)
            {
                _logger?.LogInformation($"Updated the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
            else
            {
                _logger?.LogInformation($"Failed to update the zendesk ticker number reference id: {ticketNumberReference} for case ticket id: {caseManagementTicket?.CaseTicketID}.");
            }
        }


        /// <summary>
        /// Updates the admin zendesk ticket reference and is processed status.
        /// </summary>
        /// <param name="_logger">Logger.</param>
        /// <param name="brConnectionString">BR connection string.</param>
        /// <param name="order">Order.</param>
        /// <param name="ticketNumberReference">Ticket number reference.</param>
        private static async Task UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(ILogger _logger, string brConnectionString, Order order, long ticketNumberReference, IDataLayer _dataLayer)
        {
            _logger?.LogInformation($"Updating zendesk ticket details with id :{order?.TicketId}");

            var result = await _dataLayer.ExecuteNonQueryForAdminPortal(SQLConstants.UpdateZenDeskReferenceForOTCRefundOrReshipOrders, order.OrderChangeRequestId, ticketNumberReference, brConnectionString, _logger);

            if (result == 1)
            {
                _logger?.LogInformation($"Updated the zendesk ticket number reference id: {ticketNumberReference} for change request id: {order?.OrderChangeRequestId}.");
            }
            else
            {
                _logger?.LogInformation($"Failed to update the zendesk ticker number reference id: {ticketNumberReference} for change request id: {order?.OrderChangeRequestId}.");
            }
        }
    }
}
