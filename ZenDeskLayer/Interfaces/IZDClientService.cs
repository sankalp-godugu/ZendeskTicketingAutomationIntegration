using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskTicketProcessJob.ZenDeskLayer.Interfaces
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

        /// <summary>
        /// Creates the admin ticket in zendesk asychronously.
        /// </summary>
        /// <param name="order">Order.<see cref="Order"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<long> CreateAdminTicketInZenDeskAsync(Order order, ILogger logger);

        /// <summary>
        /// Update the admin ticket in zendesk.
        /// </summary>
        /// <param name="order">Order.<see cref="Order"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public Task<long> UpdateAdminTicketInZenDeskAsync(Order order, ILogger logger);
    }
}
