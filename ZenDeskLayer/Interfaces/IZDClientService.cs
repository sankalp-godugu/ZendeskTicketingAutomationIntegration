using System.Collections.Generic;
using System.Threading.Tasks;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskAutomation.ZenDeskLayer.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IZDClientService
    {
        /// <summary>
        /// Creates the ticket in zendesk asychronously.
        /// </summary>
        /// <returns>Returns the response model on success; exception on failure.</returns>
        public Task<long> CreateTicketInZenDeskAsync(CaseTickets caseTickets);

        /// <summary>
        /// Update the ticket in zendesk.
        /// </summary>
        /// <param name="caseTicket">Case ticket.</param>
        public Task<long> UpdateTicketInZenDeskAsync(CaseTickets caseTicket);


        /// <summary>
        /// Gets the list of tickets from zendesk.
        /// </summary>
        public List<string> GetListOfTickets();
    }
}
