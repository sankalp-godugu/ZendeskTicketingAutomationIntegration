using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        /// <returns>Returns the response model on success; exception on failure.</returns>
        public async Task<long> CreateTicketInZenDeskAsync(CaseTickets caseTicket)
        {
            StringContent content = GetRequestBodyForZenDesk(caseTicket);

            // HttpClient
            HttpClient httpClient = GetZenDeskHttpClient();

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
                // Handle the error (e.g., log or throw an exception)
                throw new Exception($"Failed to call the API. Status code: {response.StatusCode}");
            }
        }
    

        /// <summary>
        /// Gets the list of tickets.
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfTickets()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Update the ticket in zendesk.
        /// </summary>
        /// <param name="caseTicket">Case ticket.</param>
        public async Task<long> UpdateTicketInZenDeskAsync(CaseTickets caseTicket)
        {
            StringContent content = GetRequestBodyForZenDesk(caseTicket);

            // HttpClient
            HttpClient httpClient = GetZenDeskHttpClient();

            // Make the API request
            HttpResponseMessage response = await httpClient.PutAsync(_configuration["ZenDesk:ApiEndPoints:UpdateTicket"] + caseTicket.ZendeskTicket, content);

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
                // Handle the error (e.g., log or throw an exception)
                throw new Exception($"Failed to call the API. Status code: {response.StatusCode}");
            }
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

        private string GetTagValueFromCarrierName(string insuranceCarrierName, long insuranceCarrierID)
        {
            string carrierName = (insuranceCarrierName ?? string.Empty).ToString()?.Replace(" ", "").Trim().ToLower();
            return $"carrier_{carrierName}_{insuranceCarrierID}";
        }


        /// <summary>
        /// Gets the request body for zendesk.
        /// </summary>
        /// <param name="caseTicket">Case ticket.</param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetRequestBodyForZenDesk(CaseTickets caseTicket)
        {
            string zenDeskSubject = $"Member ID: {caseTicket?.NHMemberID} - Case Topic: {caseTicket?.CaseTopic}";

            //Fetches the values from the configuration.
            string brandValue = _configuration["BrandValue"] ?? "16807551788311";
            string ticketFormValue = _configuration["TicketFormValue"] ?? "18750942842647";
            string nhMemberID = _configuration["NHMemberID"] ?? "17909776781591";
            string assignee = _configuration["Assignee"] ?? "16807583954071";
            string memberName = _configuration["MemberName"] ?? "18660702946583";
            string carrierName = _configuration["Carrier"] ?? "18613496572055";
            string carrierTag = GetTagValueFromCarrierName(caseTicket.InsuranceCarrierName, caseTicket.InsuranceCarrierID);
            string requestType = _configuration["RequestType"] ?? "18660741950743";
            string requestTag = NamesWithTagsConstants.GetTagValueByRequestorType(caseTicket.RequestorTypeID);
            string healthPlan = _configuration["HealthPlanName"] ?? "18660737611543";


            // Create the dynamic object
            var dynamicTicket = new
            {
                ticket = new
                {
                    assignee_email = _configuration["Email"],
                    brand_id = brandValue,
                    description = GetTicketDescriptionFromCaseTopic(caseTicket),
                    custom_fields = new[]
                    {
                        new { id = nhMemberID, value = caseTicket?.NHMemberID },
                        new { id = memberName, value = caseTicket?.MemberName },
                        new { id = carrierName, value = carrierTag },
                        new { id = assignee, value = caseTicket.AssignedTo },
                        new { id = requestType, value = requestTag },
                        new { id = healthPlan, value = caseTicket?.HealthPlanName }
                    },
                    email_ccs = new[]
                    {
                        new { user_email = _configuration["Email"], action = "put" }
                    },
                    priority = "high",
                    requester = new { email = _configuration["Email"] },
                    custom_status_id = NamesWithTagsConstants.GetTagValueByTicketStatus(caseTicket.CaseTicketStatus),
                    subject = zenDeskSubject,
                    ticket_form_id = ticketFormValue,
                    tags = new List<string>(),
                    comment = caseTicket?.ZendeskTicket != null && caseTicket?.ZendeskTicket?.Length > 0 ? GetTicketDescriptionFromCaseTopic(caseTicket) : null
                }
            };

            // Serialize the dynamic object to JSON
            string jsonPayload = JsonConvert.SerializeObject(dynamicTicket, Formatting.Indented);

            // Create StringContent from JSON payload
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return content;
        }

        private string GetTicketDescriptionFromCaseTopic(CaseTickets caseTickets)
        {
            switch (caseTickets?.CaseTopic)
            {
                case CaseTopicConstants.ItemRelatedIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForOTCCaseTopic(caseTickets);

                case CaseTopicConstants.ShipmentRelatedIssues:
                    return "Description for Shipment related issues";

                case CaseTopicConstants.HearingAidIssues:
                    return _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets);

                case CaseTopicConstants.ProviderIssues:
                    return "Description for Provider issues";

                case CaseTopicConstants.BillingIssues:
                    return "Description for Billing issues";

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
                    return _schemaTemplateService.GetSchemaDefinitionForHearingAidCaseTopic(caseTickets);

                case CaseTopicConstants.Reimbursement:
                    return _schemaTemplateService.GetSchemaDefinitionForReimbursementRequestCaseTopic(caseTickets);

                case CaseTopicConstants.WalletTransfer:
                    return _schemaTemplateService.GetSchemaDefinitionForWalletTransferCaseTopic(caseTickets);

                case CaseTopicConstants.CardReplacement:
                    return _schemaTemplateService.GetSchemaDefinitionForCardReplacementCaseTopic(caseTickets);

                case CaseTopicConstants.CardholderAddressUpdate:
                    return _schemaTemplateService.GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(caseTickets);

                case CaseTopicConstants.ChangeCardStatus:
                    return _schemaTemplateService.GetSchemaDefinitionForChangeCardStatusCaseTopic(caseTickets);

                case CaseTopicConstants.RequestVoucher:
                    return "Description for Request Voucher";

                case CaseTopicConstants.CardDeclined:
                    return "Description for Card Declined";

                case CaseTopicConstants.FlexIssue:
                    return "Description for Flex Issue";

                default:
                    return "Unknown Case Topic";
            }
        }



        #endregion
    }
}
