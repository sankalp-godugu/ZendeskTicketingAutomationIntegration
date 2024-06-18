using System;

namespace ZendeskTicketProcessingJobCMT.Models
{
    /// <summary>
    /// Case ticket.
    /// </summary>
    public class CaseTickets
    {
        /// <summary>
        /// CaseID
        /// </summary>
        public long? CaseID { get; set; }

        /// <summary>
        /// CaseNumber
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// CreateDate
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// CreateUser
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// MemberName
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// CaseCategory
        /// </summary>
        public string CaseCategory { get; set; }

        /// <summary>
        /// CaseType
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// CaseTopic
        /// </summary>
        public string CaseTopic { get; set; }

        /// <summary>
        /// NHMemberID
        /// </summary>
        public string NHMemberID { get; set; }

        /// <summary>
        /// HealthPlanName
        /// </summary>
        public string HealthPlanName { get; set; }

        /// <summary>
        /// RequestorName
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// RequestorTypeID
        /// </summary>
        public int RequestorTypeID { get; set; }

        /// <summary>
        /// MemberRelationID
        /// </summary>
        public int MemberRelationID { get; set; }

        /// <summary>
        /// MemberRelation
        /// </summary>
        public string MemberRelation { get; set; }

        /// <summary>
        /// CaseTicketID
        /// </summary>
        public long? CaseTicketID { get; set; }

        /// <summary>
        /// CaseTicketNumber
        /// </summary>
        public string CaseTicketNumber { get; set; }

        /// <summary>
        /// CaseTopicID
        /// </summary>
        public int CaseTopicID { get; set; }

        /// <summary>
        /// IsFirstCallResolution
        /// </summary>
        public bool? IsFirstCallResolution { get; set; }

        /// <summary>
        /// FirstCallResolutionDesc
        /// </summary>
        public string FirstCallResolutionDesc { get; set; }

        /// <summary>
        /// IsWrittenResolutionRequested
        /// </summary>
        public bool? IsWrittenResolutionRequested { get; set; }

        /// <summary>
        /// AssignedTo
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        /// EscalatedTo
        /// </summary>
        public string EscalatedTo { get; set; }

        /// <summary>
        /// DueDate
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// CaseTicketStatusID
        /// </summary>
        public int CaseTicketStatusID { get; set; }

        /// <summary>
        /// CaseTicketStatus
        /// </summary>
        public string CaseTicketStatus { get; set; }

        /// <summary>
        /// OrderID
        /// </summary>
        public long? OrderID { get; set; }

        /// <summary>
        /// CreatedTeam
        /// </summary>
        public string CreatedTeam { get; set; }

        /// <summary>
        /// AdditionalInfo
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// CaseTicketData
        /// </summary>
        public string CaseTicketData { get; set; }

        /// <summary>
        /// CardLast4digits
        /// </summary>
        public string CardLast4digits { get; set; }

        /// <summary>
        /// ApprovedStatus
        /// </summary>
        public string ApprovedStatus { get; set; }

        /// <summary>
        /// TransactionType 
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Ticket reason 
        /// </summary>
        public string TicketReason { get; set; }

        /// <summary>
        /// HealthPlanInsuranceId
        /// </summary>
        public string HealthPlanCaseId { get; set; }

        /// <summary>
        /// MemberSatifaction
        /// </summary>
        public string MemberSatisfaction { get; set; }

        /// <summary>
        /// CaseTypeID
        /// </summary>
        public int CaseTypeID { get; set; }

        /// <summary>
        /// FlexCaseTicketId
        /// </summary>
        public long? FlexCaseTicketId { get; set; }

        /// <summary>
        /// StatusReason
        /// </summary>
        public string StatusReason { get; set; }

        /// <summary>
        /// CreateUserFullName
        /// </summary>
        public string CreateUserFullName { get; set; }

        /// <summary>
        /// AssignToFullName
        /// </summary>
        public string AssignToFullName { get; set; }

        /// <summary>
        /// InsuranceCarrierName
        /// </summary>
        public string InsuranceCarrierName { get; set; }

        /// <summary>
        /// Insurance carrier id.
        /// </summary>
        public long? InsuranceCarrierID { get; set; }

        /// <summary>
        /// InsuranceNumber
        /// </summary>
        public string InsuranceNumber { get; set; }

        /// <summary>
        /// ClosedDate
        /// </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// ContactMethod
        /// </summary>
        public string ContactMethod { get; set; }

        /// <summary>
        /// DissatisfactionReason
        /// </summary>
        public string DissatisfactionReason { get; set; }

        /// <summary>
        /// InTakeResponseSentDate
        /// </summary>
        public DateTime? InTakeResponseSentDate { get; set; }

        /// <summary>
        /// ClosureResponseSentDate
        /// </summary>
        public DateTime? ClosureResponseSentDate { get; set; }

        /// <summary>
        /// DenialReason
        /// </summary>
        public string DenialReason { get; set; }

        /// <summary>
        /// IssueCaseType
        /// </summary>
        public string IssueCaseType { get; set; }

        /// <summary>
        /// Zendesk ticket.
        /// </summary>
        public string ZendeskTicket { get; set; }
    }
}
