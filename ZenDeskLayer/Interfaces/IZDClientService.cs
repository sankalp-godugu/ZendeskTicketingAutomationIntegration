using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenDeskAutomation.Models;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskAutomation.ZenDeskLayer.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IZDClientService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<string> CreateTicketInZenDeskAsync(CaseTickets caseTickets);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string UpdateTicketInZenDesk();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfTickets();
    }
}
