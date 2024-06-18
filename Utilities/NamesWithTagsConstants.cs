using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Utilities
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
                { ZenDeskTicketStatusConstants.New, configuration["TicketStatuses:New"] },
                { ZenDeskTicketStatusConstants.Reviewed, configuration["TicketStatuses:Reviewed"] },
                { ZenDeskTicketStatusConstants.ClosedPartially, configuration["TicketStatuses:Closed Partially"] },
                { ZenDeskTicketStatusConstants.InReview, configuration["TicketStatuses:In Review"] },
                { ZenDeskTicketStatusConstants.PendingProcessing, configuration["TicketStatuses:Pending Processing"] },
                { ZenDeskTicketStatusConstants.Pending, configuration["TicketStatuses:Pending"] },
                { ZenDeskTicketStatusConstants.Closed,configuration["TicketStatuses:Closed"]  },
                { ZenDeskTicketStatusConstants.Solved, configuration["TicketStatuses:Solved"] },
                { ZenDeskTicketStatusConstants.Failed,configuration["TicketStatuses:Failed"]  },
                { ZenDeskTicketStatusConstants.ClosedApproved, configuration["TicketStatuses:Closed Approved"]  },
                { ZenDeskTicketStatusConstants.ClosedDeclined, configuration["TicketStatuses:Closed Declined"]  }
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
