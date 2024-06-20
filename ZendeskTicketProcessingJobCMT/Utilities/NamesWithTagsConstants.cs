using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ZendeskTicketProcessingJobCMT.Utilities
{
    /// <summary>
    /// Names with tags constants.
    /// </summary>
    public static class NamesWithTagsConstants
    {
        /// <summary>
        /// ticket status ids.
        /// </summary>
        private static Dictionary<string, string> ticketStatusIds;

        /// <summary>
        /// Initializes the ticket status IDs from configuration.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        public static void Initialize(IConfiguration configuration)
        {
            ticketStatusIds = new Dictionary<string, string>
            {
                { ZendeskTicketStatusConstants.New, configuration["TicketStatuses:New"] },
                { ZendeskTicketStatusConstants.Reviewed, configuration["TicketStatuses:Reviewed"] },
                { ZendeskTicketStatusConstants.ClosedPartially, configuration["TicketStatuses:Closed Partially"] },
                { ZendeskTicketStatusConstants.InReview, configuration["TicketStatuses:In Review"] },
                { ZendeskTicketStatusConstants.PendingProcessing, configuration["TicketStatuses:Pending Processing"] },
                { ZendeskTicketStatusConstants.Pending, configuration["TicketStatuses:Pending"] },
                { ZendeskTicketStatusConstants.Closed,configuration["TicketStatuses:Closed"]  },
                { ZendeskTicketStatusConstants.Solved, configuration["TicketStatuses:Solved"] },
                { ZendeskTicketStatusConstants.Failed,configuration["TicketStatuses:Failed"]  },
                { ZendeskTicketStatusConstants.ClosedApproved, configuration["TicketStatuses:Closed Approved"]  },
                { ZendeskTicketStatusConstants.ClosedDeclined, configuration["TicketStatuses:Closed Declined"]  }
            };
        }

        /// <summary>
        /// Gets the tag value by the ticket status.
        /// </summary>
        /// <param name="ticketStatus">Ticket status.</param>
        /// <returns>Returns the field value.</returns>
        public static string GetTagValueByTicketStatus(string ticketStatus)
        {
            return ticketStatusIds.TryGetValue(ticketStatus?.ToString()?.TrimEnd(), out string tagValue) ? tagValue : string.Empty;
        }
    }
}
