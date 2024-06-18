using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.Utilities;
using ZenDeskTicketProcessJob.ZenDeskLayer.Interfaces;

namespace ZenDeskTicketProcessJob.ZenDeskLayer.Services
{
    /// <summary>
    /// Zendesk client service.
    /// </summary>
    public class ZDClientService : IZDClientService
    {
        #region Private Fields
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ISchemaTemplateService _schemaTemplateService;
        #endregion

        #region Constructor

        /// <summary>
        /// Zendesk client service service initialization.
        /// </summary>
        /// <param name="httpClientFactory">Http client factory. <see cref="IHttpClientFactory"/></param>
        /// <param name="configuration">Configuration. <see cref="IConfiguration"/></param>
        public ZDClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ISchemaTemplateService schemaTemplateService)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _schemaTemplateService = schemaTemplateService ?? throw new ArgumentNullException(nameof(schemaTemplateService));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the admin ticket in zendesk asychronously.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public async Task<long> CreateAdminTicketInZenDeskAsync(Order order, ILogger logger)
        {
            // Gets the request body for the zendesk client.
            StringContent content = GetAdminRequestBodyForZenDesk(order, logger);
            return await CreateZendeskTicketWithPassedInformation(logger, content);
        }

        /// <summary>
        /// Update the admin ticket in zendesk.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public async Task<long> UpdateAdminTicketInZenDeskAsync(Order order, ILogger logger)
        {
            // Gets the request body for the zendesk API request.
            StringContent content = GetAdminRequestBodyForZenDesk(order, logger);
            return await UpdateExistingZendeskTicketWithStringContent(order.TicketId, logger, content);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the zen desk http client.
        /// </summary>
        /// <returns>Returns the http client to make API requests.</returns>
        private HttpClient GetZenDeskHttpClient()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            // Use the configured base URL from options
            httpClient.BaseAddress = new Uri(_configuration["ZenDesk:AppConfigurations:BaseURL"]);

            // Get username and password from options
            string username = _configuration["ZenDesk:AppConfigurations:UserName"];
            string password = _configuration["ZenDesk:AppConfigurations:Password"];

            // Convert the username and password to Base64
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            // Set the Authorization header with the Basic authentication credentials
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);

            return httpClient;
        }

        /// <summary>
        /// Gets the tag value from the carrier name and carrier id.
        /// </summary>
        /// <param name="insuranceCarrierName">Insurance carrier name.</param>
        /// <param name="insuranceCarrierID">Insurance carrier id.</param>
        /// <returns>Returns the tag value from carrier name.</returns>
        private string GetTagValueFromCarrierName(string insuranceCarrierName, long? insuranceCarrierID)
        {
            string carrierName = (insuranceCarrierName ?? string.Empty).ToString()?.Replace(" ", "").Trim().ToLower();
            return $"carrier_{carrierName}_{insuranceCarrierID}";
        }

        /// <summary>
        /// Gets the admin request body for zendesk.
        /// </summary>
        /// <param name="order">Case ticket.<see cref="Order"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetAdminRequestBodyForZenDesk(Order order, ILogger logger)
        {
            try
            {
                // Constructs the zendesk fields.
                string zenDeskSubject = $"Member ID: {order?.NHMemberId} - Request Type: {order?.RequestType}";

                string carrierTag = GetTagValueFromCarrierName(order.CarrierName, order.InsuranceCarrierId);

                // Constructs the description for the OTC orders.
                string OrderID = order?.OrderId.ToString() ?? string.Empty;
                string CarrierName = order?.CarrierName ?? string.Empty;
                string NHMemberID = order?.NHMemberId.ToString() ?? string.Empty;
                string MemberName = order?.UserName?.ToString() ?? string.Empty;
                string RequestedDate = order?.RequestedDate ?? string.Empty;
                string SubmittedBy = order?.SubmittedBy ?? string.Empty;
                string RequestType = order?.RequestType ?? string.Empty;

                // Admin or rejected comments information.
                StringBuilder adminOrRejectedCommentsInformation = new();

                if (order.AdminComments != null)
                {
                    AdminComments adminComments = JsonConvert.DeserializeObject<AdminComments>(order.AdminComments);

                    if (order.Status.ToUpper() is NBTicketStatusConstants.APPROVED or NBTicketStatusConstants.REJECTED)
                    {
                        string statusString = order.Status.ToUpper() == NBTicketStatusConstants.REJECTED ? "Rejected" : "Approved";

                        _ = adminOrRejectedCommentsInformation.AppendLine($"{statusString} & Comments");
                        _ = adminOrRejectedCommentsInformation.AppendLine($"{adminComments.DisplayName} on {adminComments.Date}");

                        _ = order.Status.ToUpper() == NBTicketStatusConstants.REJECTED
                            ? adminOrRejectedCommentsInformation.AppendLine($"Rejection Reason: {adminComments.Comment}")
                            : order.Status.ToUpper() == NBTicketStatusConstants.APPROVED
                                ? adminOrRejectedCommentsInformation.AppendLine($"Approval Reason: Approved")
                                : adminOrRejectedCommentsInformation.AppendLine();
                    }
                    else
                    {
                        _ = adminOrRejectedCommentsInformation.AppendLine(string.Empty);
                    }
                }

                // Order management information.
                StringBuilder orderManagementInformation = new();
                List<ItemDetail> ItemDetails = order.ItemDetails != null ? JsonConvert.DeserializeObject<List<ItemDetail>>(order.ItemDetails) : new List<ItemDetail>();
                List<ItemComment> ItemComments = order.ItemComments != null ? JsonConvert.DeserializeObject<List<ItemComment>>(order.ItemComments) : new List<ItemComment>();

                foreach (ItemDetail itemDetail in ItemDetails)
                {
                    _ = orderManagementInformation.AppendLine();
                    _ = orderManagementInformation.AppendLine($"Item Name: {itemDetail?.ItemName}");
                    _ = orderManagementInformation.AppendLine($"Units: {itemDetail?.Quantity}");
                    _ = orderManagementInformation.AppendLine($"Unit Price: {itemDetail?.UnitPrice}");
                    _ = orderManagementInformation.AppendLine($"Total Price: {itemDetail?.TotalPrice}");
                    _ = orderManagementInformation.AppendLine($"Reason & Comments");
                    _ = orderManagementInformation.AppendLine(ItemComments.FirstOrDefault(ic => ic.OrderItemId == itemDetail.OrderItemId)?.Reason?.ToString());
                    _ = orderManagementInformation.AppendLine(ItemComments.FirstOrDefault(ic => ic.OrderItemId == itemDetail.OrderItemId)?.Comments?.ToString());
                    _ = orderManagementInformation.AppendLine();
                }

                string descriptionOrComment = $"Order ID: {OrderID}\n" +
                   $"Status: {order.Status}\n" +
                   $"Carrier Name: {CarrierName}\n" +
                   $"NHMemberID: {NHMemberID}\n" +
                   $"Member Name: {MemberName}\n" +
                   $"Requested Date: {RequestedDate}\n" +
                   $"Submitted By: {SubmittedBy}\n" +
                   $"Request Type: {RequestType}\n" +
                   adminOrRejectedCommentsInformation +
                   $"Product Details: {orderManagementInformation}\n";


                // Create the dynamic object
                var dynamicTicket = new
                {
                    ticket = new
                    {
                        assignee_email = _configuration["Email"],
                        brand_id = _configuration["BrandValue"],
                        group_id = _configuration["GroupValue"],
                        description = descriptionOrComment,
                        custom_fields = new[]
                        {
                            new { id = _configuration["NH/EHID"], value = order?.NHMemberId },
                            new { id = _configuration["MemberName"], value = order?.UserName },
                            new { id = _configuration["CarrierName-FromNBDb"], value = carrierTag }
                        },
                        email_ccs = new[]
                            {
                                new { user_email = _configuration["Email"], action = "put" }
                            },
                        priority = "high",
                        requester = new { email = _configuration["Email"] },
                        custom_status_id = GetCustomStatusIdForOTCOrders(order.Status),
                        subject = zenDeskSubject,
                        ticket_form_id = _configuration["TicketFormValue"],
                        tags = new List<string>(),
                        comment = new { body = order?.TicketId != null && order?.TicketId?.Length > 0 ? descriptionOrComment : null }
                    }
                };

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(dynamicTicket, Formatting.Indented);

                // Create StringContent from JSON payload
                StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the request body for zendesk with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the custom status for OTC refund and reship orders.
        /// </summary>
        /// <param name="status">Status</param>
        /// <returns>Returns the field value from the order status.</returns>
        public string GetCustomStatusIdForOTCOrders(string status)
        {
            string uppercasedStatus = status?.ToUpper() ?? string.Empty;

            return uppercasedStatus switch
            {
                NBTicketStatusConstants.PENDING => NamesWithTagsConstants.GetTagValueByTicketStatus(ZenDeskTicketStatusConstants.New),
                NBTicketStatusConstants.APPROVED or NBTicketStatusConstants.REJECTED => NamesWithTagsConstants.GetTagValueByTicketStatus(ZenDeskTicketStatusConstants.Closed),
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Updates the existing zendesk ticket with string content.
        /// </summary>
        /// <param name="zendeskTicketId">Zendesk ticket id.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="content">Sting content.</param>
        /// <returns>Returns the updated ticket id.</returns>
        private async Task<long> UpdateExistingZendeskTicketWithStringContent(string zendeskTicketId, ILogger logger, StringContent content)
        {
            if (content != null)
            {
                // HttpClient
                using HttpClient httpClient = GetZenDeskHttpClient();

                // Make the API request
                HttpResponseMessage response = await httpClient.PutAsync(_configuration["ZenDesk:ApiEndPoints:UpdateTicket"] + zendeskTicketId, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseContent = await response?.Content?.ReadAsStringAsync();

                    // Deserialize the JSON string
                    JObject jsonResponse = JObject.Parse(responseContent);

                    // Get the value of a specific property
                    long ticketIdentifier = Convert.ToInt64(jsonResponse["ticket"]["id"]);

                    // Return the ticket identifier.
                    return ticketIdentifier;
                }
                else
                {
                    logger.LogError($"Failed to call the update zendesk API with response: {response}");
                    return 0;
                }
            }
            else { return 0; }
        }

        /// <summary>
        /// Creates the zendesk ticket with the passed body information.
        /// </summary>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <param name="content">Content.<see cref="StringContent"/></param>
        /// <returns>Returns the created ticket id from the zendesk.</returns>
        private async Task<long> CreateZendeskTicketWithPassedInformation(ILogger logger, StringContent content)
        {
            // Gets the zendesk http client.
            using HttpClient httpClient = GetZenDeskHttpClient();
            // Make the API request
            HttpResponseMessage response = await httpClient.PostAsync(_configuration["ZenDesk:ApiEndPoints:CreateTicket"], content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string
                JObject jsonResponse = JObject.Parse(responseContent);

                // Get the value of a specific property
                long ticketIdentifier = Convert.ToInt64(jsonResponse["ticket"]["id"]);

                // Return the deserialized response
                return ticketIdentifier;
            }
            else
            {
                logger.LogError($"Failed to call the create zendesk API with response: {response}");
                return 0;
            }
        }

        public Task<long> CreateCMTTicketInZenDeskAsync(CaseTickets caseTickets, ILogger logger)
        {
            throw new NotImplementedException();
        }

        public Task<long> UpdateCMTTicketInZenDeskAsync(CaseTickets caseTicket, ILogger logger)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
