using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZendeskTicketProcessingJobCMT.Models;
using ZendeskTicketProcessingJobCMT.SchemaTemplateLayer.Interfaces;
using ZendeskTicketProcessingJobCMT.Utilities;
using ZendeskTicketProcessingJobCMT.ZendeskLayer.Interfaces;

namespace ZendeskTicketProcessingJobCMT.ZendeskLayer.Services
{
    /// <summary>
    /// Zendesk client service.
    /// </summary>
    public class ZDClientService : IZDClientService
    {
        #region Private Fields
        private IHttpClientFactory _httpClientFactory;
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
            httpClient.BaseAddress = new Uri(_configuration["Zendesk:AppConfigurations:BaseURL"]);

            // Get username and password from options
            string username = _configuration["Zendesk:AppConfigurations:UserName"];
            string password = _configuration["Zendesk:AppConfigurations:Password"];

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

                string descriptionOrComment = GetTicketDescriptionFromCaseTopic(caseTicket, logger);

                // Create the dynamic object
                var dynamicTicket = new
                {
                    ticket = new
                    {
                        assignee_email = _configuration["Email"],
                        brand_id = _configuration["BrandId"],
                        group_id = _configuration["GroupId"],
                        description = descriptionOrComment,
                        custom_fields = new[]
                        {
                            new { id = _configuration["NhId"], value = caseTicket?.NHMemberID },
                            new { id = _configuration["MemberName"], value = caseTicket?.MemberName },
                            new { id = _configuration["CarrierName-FromNBDb"], value = carrierTag },
                            //new { id = _configuration["Assignee"], value = caseTicket.AssignedTo },
                            new { id = _configuration["PlanName"], value = caseTicket?.HealthPlanName },
                        },
                        email_ccs = new[]
                            {
                            new { user_email = _configuration["Email"], action = "put" }
                        },
                        priority = "high",
                        requester = new { email = _configuration["Email"] },
                        custom_status_id = caseTicket.CaseTopic is CaseTopicConstants.Reimbursement or CaseTopicConstants.WalletTransfer ? NamesWithTagsConstants.GetTagValueByTicketStatus(caseTicket?.CaseTicketStatus) : NamesWithTagsConstants.GetTagValueByTicketStatus(caseTicket?.CaseTicketStatus + " " + caseTicket?.ApprovedStatus),
                        subject = zenDeskSubject,
                        ticket_form_id = _configuration["TicketFormId"],
                        tags = new List<string>(),
                        comment = new { body = caseTicket?.ZendeskTicket != null && caseTicket?.ZendeskTicket?.Length > 0 ? descriptionOrComment : null }
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
        /// Gets the ticket description from the case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the ticket description from the case topic.</returns>
        private string GetTicketDescriptionFromCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            return (caseTickets?.CaseTopic) switch
            {
                CaseTopicConstants.ItemRelatedIssues => _schemaTemplateService.GetSchemaDefinitionForOTCCaseTopic(caseTickets, logger),
                CaseTopicConstants.ShipmentRelatedIssues => _schemaTemplateService.GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(caseTickets, logger),
                CaseTopicConstants.HearingAidIssues => _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets, logger),
                CaseTopicConstants.ProviderIssues => _schemaTemplateService.GetSchemaDefinitionForProviderIssuesCaseTopic(caseTickets, logger),
                CaseTopicConstants.BillingIssues => _schemaTemplateService.GetSchemaDefinitionForBillingIssuesCaseTopic(caseTickets, logger),
                CaseTopicConstants.UserAgreementsNotReceived => "Description for User Agreements (Not received)",
                CaseTopicConstants.WrongItemReceived => "Description for Wrong Item received",
                CaseTopicConstants.DeviceIssue => "Description for Device Issue",
                CaseTopicConstants.BalanceNotLoaded => "Description for Balance not loaded",
                CaseTopicConstants.WrongWalletCharged => "Description for Wrong wallet charged",
                CaseTopicConstants.TransactionDeclined => "Description for Transaction declined",
                CaseTopicConstants.Others => _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets, logger),
                CaseTopicConstants.Reimbursement => _schemaTemplateService.GetSchemaDefinitionForReimbursementRequestCaseTopic(caseTickets, logger),
                CaseTopicConstants.WalletTransfer => _schemaTemplateService.GetSchemaDefinitionForWalletTransferCaseTopic(caseTickets, logger),
                CaseTopicConstants.CardReplacement => _schemaTemplateService.GetSchemaDefinitionForCardReplacementCaseTopic(caseTickets, logger),
                CaseTopicConstants.CardholderAddressUpdate => _schemaTemplateService.GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(caseTickets, logger),
                CaseTopicConstants.ChangeCardStatus => _schemaTemplateService.GetSchemaDefinitionForChangeCardStatusCaseTopic(caseTickets, logger),
                CaseTopicConstants.RequestVoucher => "Description for Request Voucher",
                CaseTopicConstants.CardDeclined => _schemaTemplateService.GetSchemaDefinitionForCardDeclinedCaseTopic(caseTickets, logger),
                CaseTopicConstants.FlexIssue => _schemaTemplateService.GetSchemaDefinitionForFlexIssueCaseTopic(caseTickets, logger),
                _ => "Unknown Case Topic",
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
                HttpResponseMessage response = await httpClient.PutAsync(_configuration["Zendesk:ApiEndPoints:UpdateTicket"] + zendeskTicketId, content);

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
            HttpResponseMessage response = await httpClient.PostAsync(_configuration["Zendesk:ApiEndPoints:CreateTicket"], content);

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

        #endregion
    }
}
