using Microsoft.Extensions.Logging;
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
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<long> CreateTicketInZenDeskAsync(CaseTickets caseTickets, ILogger logger);

        /// <summary>
        /// Update the ticket in zendesk.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public Task<long> UpdateTicketInZenDeskAsync(CaseTickets caseTicket, ILogger logger);
    }
}
