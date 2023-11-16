using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenDeskTicketProcessJob.Models
{
    /// <summary>
    /// Case ticket.
    /// </summary>
    public class CaseTickets
    {
        /// <summary>
        /// Case ticket id.
        /// </summary>
        public long CaseTicketID { get; set; }

        /// <summary>
        /// Case ticket number.
        /// </summary>
        public string CaseTicketNumber { get; set;}

        /// <summary>
        /// Case identifier.
        /// </summary>
        public long CaseID { get; set; }

        /// <summary>
        /// Assigned to.
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        /// Closing comments.
        /// </summary>
        public string ClosingComments { get; set; }
    }
}
