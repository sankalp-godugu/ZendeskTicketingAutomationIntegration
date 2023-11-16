using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZenDeskAutomation.Models;
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
        public async Task<string> CreateTicketInZenDeskAsync(CaseTickets caseTicket)
        {
            // Assuming caseTicket is a variable of some class or type
            string assigneeEmail = "spuramsetti@nationsbenefits.com";
            string closingComments = "Closing the ticket with ticket number " + caseTicket.CaseTicketNumber;
            string caseTicketNumber = caseTicket.CaseTicketNumber;

            // Create JSON payload
            string jsonPayload = @"{
                ""ticket"": {
                     ""assignee_email"": """ + assigneeEmail + @""",
                     ""description"": """ + closingComments + @""",
                     ""subject"": """ + caseTicketNumber + @"""
                }
            }";
            // Create StringContent from JSON payload
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


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
                JObject jsonObject = JObject.Parse(responseContent);

                // Get the value of a specific property
                string ticketIdentifier = jsonObject["ticket:id"]?.ToString();

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
        public string UpdateTicketInZenDesk()
        {
            throw new System.NotImplementedException();
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


        #endregion
    }
}
