using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZenDeskTicketProcessJob.DataLayer.Interfaces;
using ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.Utilities;
using ZenDeskTicketProcessJob.ZenDeskLayer.Interfaces;

namespace ZenDeskTicketProcessJob.TriggerUtilities
{

    /// <summary>
    /// Zendesk utilities.
    /// </summary>
    public static class ZenDeskTicketUtilities
    {
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
                _ = Task.Run(async () =>
                {
                    _logger?.LogInformation("********* Admin Portal => ZenDesk Execution Started **********");

                    // CRM connection string.
                    string CRMConnectionString = _configuration["DataBase:CRMConnectionString"];

                    // SQL parameters.
                    Dictionary<string, object> sqlParams = new()
                    {
                        {"@date", _configuration["AdminCurrentDate"]},
                        {"@count", _configuration["AdminCount"] }
                    };

                    _logger?.LogInformation("Started fetching the refund and reship over the counter orders");

                    List<OrderChangeRequest> orderChangeRequestIds = await _dataLayer.ExecuteReader<OrderChangeRequest>(SQLConstants.GetOrderChangeRequestsForZenDeskIntegration, sqlParams, CRMConnectionString, _logger);

                    _logger?.LogInformation($"Ended fetching the refund and reship over the counter orders with count: {orderChangeRequestIds?.Count}");

                    string brConnectionString = _configuration["DataBase:BRConnectionString"];

                    foreach (OrderChangeRequest orderChangeRequestId in orderChangeRequestIds)
                    {

                        Dictionary<string, object> keyValuePairs = new()
                        {
                            {"@OrderChangeRequestId", orderChangeRequestId.OrderChangeRequestId}
                        };

                        Order orderChangeRequest = (await _dataLayer.ExecuteReader<Order>(SQLConstants.GetOrderDetailsForZenDeskIntegrationByChangeRequestId, keyValuePairs, CRMConnectionString, _logger)).FirstOrDefault();

                        if (orderChangeRequest != null)
                        {

                            if (!string.IsNullOrWhiteSpace(orderChangeRequest?.TicketId) && Convert.ToInt64(orderChangeRequest?.TicketId) > 0)
                            {
                                _logger?.LogInformation($"Started updating ticket via zendesk API for the admin ticket id: {orderChangeRequest.TicketId} for NHMemberID {orderChangeRequest?.NHMemberId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, Convert.ToInt64(orderChangeRequest?.TicketId), 2, _dataLayer);

                                long ticketNumberReference = await _zdClientService?.UpdateAdminTicketInZenDeskAsync(orderChangeRequest, _logger);

                                _logger?.LogInformation($"Successfully updated zendesk ticket id {ticketNumberReference} for the order change request id: {orderChangeRequest.OrderChangeRequestId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, ticketNumberReference, ticketNumberReference == 0 ? 0 : 1, _dataLayer);

                            }
                            else
                            {
                                _logger?.LogInformation($"Started creating ticket via zendesk API for the order change request id: {orderChangeRequest.OrderChangeRequestId}  for NHMemberID {orderChangeRequest?.NHMemberId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, 0, 2, _dataLayer);

                                long ticketNumberReference = await _zdClientService.CreateAdminTicketInZenDeskAsync(orderChangeRequest, _logger);

                                _logger?.LogInformation($"Successfully created zendesk ticket id {ticketNumberReference} for the order change request id: {orderChangeRequest.OrderChangeRequestId} with details {orderChangeRequest}");

                                await UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, orderChangeRequest, ticketNumberReference, ticketNumberReference == 0 ? 0 : 1, _dataLayer);
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
        /// Updates the admin zendesk ticket reference and is processed status.
        /// </summary>
        /// <param name="_logger">Logger.</param>
        /// <param name="brConnectionString">BR connection string.</param>
        /// <param name="order">Order.</param>
        /// <param name="ticketNumberReference">Ticket number reference.</param>
        /// <param name="currentProcessStatus">
        /// Current process status.
        /// 0 - Not Processed.
        /// 1 - Processed.
        /// 2 - ZenDesk Processing.
        /// </param>
        private static async Task UpdatesAdminZendeskTicketReferenceAndIsProcessedStatus(ILogger _logger, string brConnectionString, Order order, long ticketNumberReference, long currentProcessStatus, IDataLayer _dataLayer)
        {
            _logger?.LogInformation($"Updating zendesk ticket details with id :{order?.TicketId}");

            int result = await _dataLayer.ExecuteNonQueryForAdminPortal(SQLConstants.UpdateZenDeskReferenceForOTCRefundOrReshipOrders, order.OrderChangeRequestId, ticketNumberReference, currentProcessStatus, brConnectionString, _logger);

            if (result == 1)
            {
                if (currentProcessStatus == 2)
                {
                    _logger?.LogInformation($"Updating IsProcessed of order change request id {order?.OrderChangeRequestId} to status(2) which says currently it is sending to zendesk for zendesk processing.");
                }
                else
                {
                    _logger?.LogInformation($"Updated the zendesk ticket number reference id: {ticketNumberReference} for change request id: {order?.OrderChangeRequestId}.");
                }
            }
            else
            {
                if (currentProcessStatus == 2)
                {
                    _logger?.LogInformation($"Failed to update the IsProcessed of order change request id {order?.OrderChangeRequestId} to status(2) which says currently it is sending to zendesk for zendesk processing.");
                }
                else
                {
                    _logger?.LogInformation($"Failed to update the zendesk ticker number reference id: {ticketNumberReference} for change request id: {order?.OrderChangeRequestId}.");
                }
            }
        }
    }
}
