using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskTicketProcessingJobCMT.Models;

namespace ZendeskTicketProcessingJobCMT.ZendeskLayer.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IZDClientService
    {
        /// <summary>
        /// Creates the CMT ticket in zendesk asychronously.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<long> CreateCMTTicketInZenDeskAsync(CaseTickets caseTickets, ILogger logger);

        /// <summary>
        /// Update the CMT ticket in zendesk.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public Task<long> UpdateCMTTicketInZenDeskAsync(CaseTickets caseTicket, ILogger logger);
    }
}
