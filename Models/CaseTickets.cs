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
        public long MemberID { get; set; }
        public string NHMemberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long InsuranceHealthPlanID { get; set; }
        public string CreateUser { get; set; }
        public string AssignedTo { get; set; }
        public long CaseID { get; set; }
        public string CaseNumber { get; set; }
        public long CaseTicketID { get; set; }
        public string CaseTicketNumber { get; set; }
        public string CaseTicketData { get; set; }
        public long RequestorTypeID { get; set; }
        public string RequestorType { get; set; }
        public long CaseSourceID { get; set; }
        public string CaseSource { get; set; }
        public long CaseTopicID { get; set; }
        public string CaseTopic { get; set; }
        public string ZendeskTicket { get; set; }
    }
}
