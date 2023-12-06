using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Utilities
{
    /// <summary>
    /// Names with tags constants.
    /// </summary>
    public static class NamesWithTagsConstants
    {
        private static readonly Dictionary<int, string> requestorTypes = new Dictionary<int, string>
        {
            {1, "who_is_contacting_member"},
            {2, "who_is_contacting_member_representative"},
            {3, "who_is_contacting_health_plan"},
        };

        private static readonly Dictionary<string, long> ticketStatusIds = new Dictionary<string, long>
        {
            { "New", 16807567164695 },
            { "Reviewed", 19295269450263 },
            { "Closed Partially", 19294912565143 },
            { "In Review", 19294761414807 },
            { "Pending Processing", 19317146798103 },
            { "Pending", 19437357728407 },
            { "Closed",19317436471447  },
            { "Failed",19294928631191  },
            { "Closed Approved", 19317038480151  },
            { "Closed Declined", 19317055615639  },
        };


        private static readonly Dictionary<string, string> issueRelatedTypes = new Dictionary<string, string>
        {
            {"OTC/Healthy Foods", "csm-team_otc_healthy_foods"},
            {"PERS", "csm-team_pers"},
            {"Transportation", "csm-team_transportation"},
            {"FLEX Card", "csm-team_flex_card_services"},
            {"Grievances", "csm-team_grievances"},
            {"Supervisors", "csm-team_supervisor_callback"},
            {"General inquiry", "csm-team_general_inquiry"},
            {"Member Related issues", "csm-team_member_related_issues"}
        };

        public static long GetTagValueByTicketStatus(string ticketStatus)
        {
            if (ticketStatusIds.TryGetValue(ticketStatus?.ToString()?.TrimEnd(), out long tagValue))
            {
                return tagValue;
            }
            else
            {
                return 0;
            }
        }

        public static string GetTagValueByRequestorType(int requestorType)
        {
            if (requestorTypes.TryGetValue(requestorType, out string tagValue))
            {
                return tagValue;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetTagValueByIssueRelated(string issueRelated)
        {
            if (issueRelatedTypes.TryGetValue(issueRelated, out string mapping))
            {
                return mapping;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
