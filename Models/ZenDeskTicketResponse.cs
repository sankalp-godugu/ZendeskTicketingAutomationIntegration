using System;
using System.Collections.Generic;

namespace ZenDeskAutomation.Models
{
    public class ZenDeskTicketResponse
    {
        public int AssigneeId { get; set; }
        public List<int> CollaboratorIds { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public int CustomStatusId { get; set; }
        public string Description { get; set; }
        public DateTime? DueAt { get; set; }
        public string ExternalId { get; set; }
        public List<int> FollowerIds { get; set; }
        public bool FromMessagingChannel { get; set; }
        public int GroupId { get; set; }
        public bool HasIncidents { get; set; }
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Priority { get; set; }
        public int ProblemId { get; set; }
        public string RawSubject { get; set; }
        public string Recipient { get; set; }
        public int RequesterId { get; set; }
        public SatisfactionRating SatisfactionRating { get; set; }
        public List<int> SharingAgreementIds { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public int SubmitterId { get; set; }
        public List<string> Tags { get; set; }
        public string Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Url { get; set; }
        public Via Via { get; set; }
    }

    public class CustomField
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class SatisfactionRating
    {
        public string Comment { get; set; }
        public int Id { get; set; }
        public string Score { get; set; }
    }

    public class Via
    {
        public string Channel { get; set; }
    }

}
