using System;
using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Models
{

    public class Via
    {
        public string Channel { get; set; }
        public Source Source { get; set; }
    }

    public class Ticket
    {
        public string Url { get; set; }
        public int Id { get; set; }
        public object ExternalId { get; set; }
        public Via Via { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string RawSubject { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public object Recipient { get; set; }
        public long RequesterId { get; set; }
        public long SubmitterId { get; set; }
        public long AssigneeId { get; set; }
        public object OrganizationId { get; set; }
        public long GroupId { get; set; }
        public List<object> CollaboratorIds { get; set; }
        public List<object> FollowerIds { get; set; }
        public List<object> EmailCcIds { get; set; }
        public object ForumTopicId { get; set; }
        public object ProblemId { get; set; }
        public bool HasIncidents { get; set; }
        public bool IsPublic { get; set; }
        public object DueAt { get; set; }
        public List<string> Tags { get; set; }
        public List<object> CustomFields { get; set; }
        public object SatisfactionRating { get; set; }
        public List<object> SharingAgreementIds { get; set; }
        public long CustomStatusId { get; set; }
        public List<object> Fields { get; set; }
        public List<object> FollowupIds { get; set; }
        public long TicketFormId { get; set; }
        public long BrandId { get; set; }
        public bool AllowChannelback { get; set; }
        public bool AllowAttachments { get; set; }
        public bool FromMessagingChannel { get; set; }
    }

    public class Source
    {
        public string From { get; set; }
        public string To { get; set; }
        public object Rel { get; set; }
    }

    public class ZenDeskTicket
    {
        public List<Ticket> Tickets { get; set; }
        public object NextPage { get; set; }
        public object PreviousPage { get; set; }
        public int Count { get; set; }
    }

}
