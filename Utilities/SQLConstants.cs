using ZenDeskTicketProcessJob.Models.ZenDeskTicketProcessJob.Models;

namespace ZenDeskAutomation.Utilities
{
    /// <summary>
    /// SQL constants.
    /// </summary>
    public class SQLConstants
    {
        /// <summary>
        /// Get OBT Benfit details.
        /// </summary>
        public static string GetAllCaseTicketsForAllMembers = 
            "[ServiceRequest].[GetAllCaseTicketsForAllMembers]";

        /// <summary>
        /// Updates the zendesk reference for member case tickets.
        /// </summary>
        public static string UpdateZenDeskReferenceForMemberCaseTickets  = "ServiceRequest.UpdateZenDeskReferenceForMemberCaseTickets";

        /// <summary>
        /// Gets the order change requests for zendesk integration.
        /// </summary>
        public static string GetOrderChangeRequestsForZenDeskIntegration =
            "[Orders].[GetOrderChangeRequestsForZenDeskIntegration]";

        /// <summary>
        /// Updates the zendesk reference for member case tickets.
        /// </summary>
        public static string UpdateZenDeskReferenceForOTCRefundOrReshipOrders = "[Orders].[UpdateZenDeskReferenceForOTCRefundOrReshipOrders]";


    }
}
