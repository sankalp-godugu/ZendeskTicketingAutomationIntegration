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
        #endregion

        #region Constructor

        /// <summary>
        /// Zendesk client service service initialization.
        /// </summary>
        /// <param name="httpClientFactory">Http client factory. <see cref="IHttpClientFactory"/></param>
        /// <param name="configuration">Configuration. <see cref="IConfiguration"/></param>
        public ZDClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

        /// <summary>
        /// Gets the request body for zendesk.
        /// </summary>
        /// <param name="caseTicket">Case ticket.</param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetRequestBodyForZenDesk(CaseTickets caseTicket)
        {
            string zenDeskSubject = $"Member ID: {caseTicket.NHMemberID} with name {caseTicket.FirstName} {caseTicket.LastName} for health plan id {caseTicket.InsuranceHealthPlanID}";

            //Fetches the values from the configuration.
            string brandValue = _configuration["BrandValue"] ?? "";
            string ticketFormValue = _configuration["TicketFormValue"] ?? "";
            string subject = _configuration["Subject"] ?? "";
            string description = _configuration["Description"] ?? "";
            string nhMemberID = _configuration["NHMemberID"] ?? "";
            string memberName = _configuration["MemberName"] ?? "";
            string carrierName = _configuration["Carrier"] ?? "";
            string contactType = _configuration["ContactType"] ?? "";
            string requestType = _configuration["RequestType"] ?? "";
            string IssueRelated = _configuration["IssueRelated"] ?? "";
            
            // Create the dynamic object
            var dynamicTicket = new
            {
                ticket = new
                {
                    assignee_email = _configuration["Email"],
                    brand_id = brandValue,
                    description = caseTicket.CaseTicketData,
                    custom_fields = new[]
                    {
                        new { id = nhMemberID, value = caseTicket?.NHMemberID },
                        new { id = memberName, value = caseTicket?.FirstName + caseTicket?.LastName }
                    },
                    email_ccs = new[]
                    {
                        new { user_email = _configuration["Email"], action = "put" }
                    },
                    priority = "low",
                    requester = new { email = _configuration["Email"] },
                    status = "new",
                    subject = zenDeskSubject,
                    ticket_form_id = ticketFormValue,
                    tags = new List<string>()
                }
            };

            // Serialize the dynamic object to JSON
            string jsonPayload = JsonConvert.SerializeObject(dynamicTicket, Formatting.Indented);

            // Create StringContent from JSON payload
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return content;
        }


        #endregion
    }
}
