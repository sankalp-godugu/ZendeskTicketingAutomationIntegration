using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZenDeskAutomation.ZenDeskLayer.Interfaces;
using ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.Utilities;

namespace ZenDeskAutomation.ZenDeskLayer.Services
{
    /// <summary>
    /// Zendesk client service.
    /// </summary>
    public class ZDClientService : IZDClientService
    {
        #region Private Fields
        private IHttpClientFactory _httpClientFactory;
        private IConfiguration _configuration;
        private ISchemaTemplateService _schemaTemplateService;
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
        /// Creates the ticket in zendesk asychronously.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public async Task<long> CreateCMTTicketInZenDeskAsync(CaseTickets caseTicket, ILogger logger)
        {
            // Gets the request body for the zendesk client.
            StringContent content = GetCMTRequestBodyForZenDesk(caseTicket, logger);
            return await CreateZendeskTicketWithPassedInformation(logger, content);
        }

        /// <summary>
        /// Update the ticket in zendesk.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public async Task<long> UpdateCMTTicketInZenDeskAsync(CaseTickets caseTicket, ILogger logger)
        {
            // Gets the request body for the zendesk API request.
            StringContent content = GetCMTRequestBodyForZenDesk(caseTicket, logger);
            return await UpdateExistingZendeskTicketWithStringContent(caseTicket.ZendeskTicket, logger, content);
        }

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
        /// Gets the CMT request body for zendesk.
        /// </summary>
        /// <param name="caseTicket">Case ticket.<see cref="CaseTickets"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetCMTRequestBodyForZenDesk(CaseTickets caseTicket, ILogger logger)
        {
            try
            {
                // Constructs the zendesk fields.
                string zenDeskSubject = $"Member ID: {caseTicket?.NHMemberID} - Case Topic: {caseTicket?.CaseTopic}";
                string carrierTag = GetTagValueFromCarrierName(caseTicket.InsuranceCarrierName, caseTicket.InsuranceCarrierID);

                var descriptionOrComment = GetTicketDescriptionFromCaseTopic(caseTicket, logger);

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
                            new { id = _configuration["NH/EHID"], value = caseTicket?.NHMemberID },
                            new { id = _configuration["MemberName"], value = caseTicket?.MemberName },
                            new { id = _configuration["CarrierName-FromNBDb"], value = carrierTag },
                            new { id = _configuration["Assignee"], value = caseTicket.AssignedTo },
                            new { id = _configuration["PlanName"], value = caseTicket?.HealthPlanName },
                        },
                        email_ccs = new[]
                            {
                            new { user_email = _configuration["Email"], action = "put" }
                        },
                        priority = "high",
                        requester = new { email = _configuration["Email"] },
                        custom_status_id = (CaseTopicConstants.Reimbursement == caseTicket.CaseTopic || CaseTopicConstants.WalletTransfer == caseTicket.CaseTopic) ? NamesWithTagsConstants.GetTagValueByTicketStatus(caseTicket?.CaseTicketStatus) : NamesWithTagsConstants.GetTagValueByTicketStatus(caseTicket?.CaseTicketStatus + " " + caseTicket?.ApprovedStatus),
                        subject = zenDeskSubject,
                        ticket_form_id = _configuration["TicketFormValue"],
                        tags = new List<string>(),
                        comment = new { body = caseTicket?.ZendeskTicket != null && caseTicket?.ZendeskTicket?.Length > 0 ? descriptionOrComment : null }
                    }
                };

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(dynamicTicket, Formatting.Indented);

                // Create StringContent from JSON payload
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the request body for zendesk with exception message: {ex.Message}");
                return null;
            }
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

                // HA Item Information
                StringBuilder orderManagementInformation = new StringBuilder();
                List<ItemDetail> ItemDetails = order.ItemDetails != null ? JsonConvert.DeserializeObject<List<ItemDetail>>(order.ItemDetails) : new List<ItemDetail>();
                List<ItemComment> ItemComments = order.ItemComments != null ? JsonConvert.DeserializeObject<List<ItemComment>>(order.ItemComments) : new List<ItemComment>();

                foreach (var itemDetail in ItemDetails)
                {
                    orderManagementInformation.AppendLine();
                    orderManagementInformation.AppendLine($"Item Name: {itemDetail?.ItemName}");
                    orderManagementInformation.AppendLine($"Units: {itemDetail?.Quantity}");
                    orderManagementInformation.AppendLine($"Unit Price: {itemDetail?.UnitPrice}");
                    orderManagementInformation.AppendLine($"Total Price: {itemDetail?.TotalPrice}");
                    orderManagementInformation.AppendLine($"Reason & Comments");
                    orderManagementInformation.AppendLine(ItemComments.FirstOrDefault(ic => ic.OrderItemId == itemDetail.OrderItemId)?.Reason?.ToString());
                    orderManagementInformation.AppendLine(ItemComments.FirstOrDefault(ic => ic.OrderItemId == itemDetail.OrderItemId)?.Comments?.ToString());

                    if (order.AdminComments != null)
                    {
                        AdminComments adminComments = JsonConvert.DeserializeObject<AdminComments>(order.AdminComments);

                        if (order.Status.ToUpper() == NBTicketStatusConstants.APPROVED || order.Status.ToUpper() == NBTicketStatusConstants.REJECTED)
                        {
                            string statusString = order.Status.ToUpper() == NBTicketStatusConstants.REJECTED ? "Rejected" : "Approved";

                            orderManagementInformation.AppendLine($"{statusString} & Comments");
                            orderManagementInformation.AppendLine($"{adminComments.DisplayName} on {adminComments.Date}");

                            if (order.Status.ToUpper() == NBTicketStatusConstants.REJECTED)
                            {
                                orderManagementInformation.AppendLine($"Reason: {adminComments.Comment}");
                            }
                            else if(order.Status.ToUpper() == NBTicketStatusConstants.APPROVED)
                            {
                                orderManagementInformation.AppendLine("Reason: Approved");
                            }
                            else
                            {
                                orderManagementInformation.AppendLine();
                            }
                        }
                        else
                        {
                            orderManagementInformation.AppendLine(string.Empty);
                        }
                    }


                    orderManagementInformation.AppendLine();
                }

                var descriptionOrComment = $"Order ID: {OrderID}\n" +
                   $"Status: {order.Status}\n" +
                   $"Carrier Name: {CarrierName}\n" +
                   $"NHMemberID: {NHMemberID}\n" +
                   $"Member Name: {MemberName}\n" +
                   $"Requested Date: {RequestedDate}\n" +
                   $"Submitted By: {SubmittedBy}\n" +
                   $"Request Type: {RequestType}\n" +
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
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
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

            switch (uppercasedStatus)
            {
                case NBTicketStatusConstants.PENDING:
                    return NamesWithTagsConstants.GetTagValueByTicketStatus(ZenDeskTicketStatusConstants.New);
                case NBTicketStatusConstants.APPROVED:
                case NBTicketStatusConstants.REJECTED:
                    return NamesWithTagsConstants.GetTagValueByTicketStatus(ZenDeskTicketStatusConstants.Closed);
                default:
                    return string.Empty;
            }
        }



        /// <summary>
        /// Gets the ticket description from the case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the ticket description from the case topic.</returns>
        private string GetTicketDescriptionFromCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            switch (caseTickets?.CaseTopic)
            {
                case CaseTopicConstants.ItemRelatedIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForOTCCaseTopic(caseTickets, logger);

                case CaseTopicConstants.ShipmentRelatedIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(caseTickets, logger);

                case CaseTopicConstants.HearingAidIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets, logger);

                case CaseTopicConstants.ProviderIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForProviderIssuesCaseTopic(caseTickets, logger);

                case CaseTopicConstants.BillingIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForBillingIssuesCaseTopic(caseTickets, logger);

                case CaseTopicConstants.UserAgreementsNotReceived:
                    return "Description for User Agreements (Not received)";

                case CaseTopicConstants.WrongItemReceived:
                    return "Description for Wrong Item received";

                case CaseTopicConstants.DeviceIssue:
                    return "Description for Device Issue";

                case CaseTopicConstants.BalanceNotLoaded:
                    return "Description for Balance not loaded";

                case CaseTopicConstants.WrongWalletCharged:
                    return "Description for Wrong wallet charged";

                case CaseTopicConstants.TransactionDeclined:
                    return "Description for Transaction declined";

                case CaseTopicConstants.Others:
                    return _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets, logger);

                case CaseTopicConstants.Reimbursement:
                    return _schemaTemplateService.GetSchemaDefinitionForReimbursementRequestCaseTopic(caseTickets, logger);

                case CaseTopicConstants.WalletTransfer:
                    return _schemaTemplateService.GetSchemaDefinitionForWalletTransferCaseTopic(caseTickets, logger);

                case CaseTopicConstants.CardReplacement:
                    return _schemaTemplateService.GetSchemaDefinitionForCardReplacementCaseTopic(caseTickets, logger);

                case CaseTopicConstants.CardholderAddressUpdate:
                    return _schemaTemplateService.GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(caseTickets, logger);

                case CaseTopicConstants.ChangeCardStatus:
                    return _schemaTemplateService.GetSchemaDefinitionForChangeCardStatusCaseTopic(caseTickets, logger);

                case CaseTopicConstants.RequestVoucher:
                    return "Description for Request Voucher";

                case CaseTopicConstants.CardDeclined:
                    return _schemaTemplateService.GetSchemaDefinitionForCardDeclinedCaseTopic(caseTickets, logger);

                case CaseTopicConstants.FlexIssue:
                    return _schemaTemplateService.GetSchemaDefinitionForFlexIssueCaseTopic(caseTickets, logger);

                default:
                    return "Unknown Case Topic";
            }
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
                using (HttpClient httpClient = GetZenDeskHttpClient())
                {

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
            using (HttpClient httpClient = GetZenDeskHttpClient())
            {
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
        }

        #endregion
    }
}
