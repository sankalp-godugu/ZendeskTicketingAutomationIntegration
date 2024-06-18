namespace ZendeskTicketProcessingJobAP.Utilities
{
    /// <summary>
    /// SQL constants.
    /// </summary>
    public class SQLConstants
    {
        /// <summary>
        /// Gets the order change requests for zendesk integration.
        /// </summary>
        public static string GetOrderChangeRequestsForZenDeskIntegration =
            "[Orders].[GetOrderChangeRequestsForZenDeskIntegration]";


        /// <summary>
        /// Gets the order change requests for zendesk integration.
        /// </summary>
        public static string GetOrderDetailsForZenDeskIntegrationByChangeRequestId =
            "[Orders].[GetOrderDetailsForZenDeskIntegrationByChangeRequestId]";

        /// <summary>
        /// Updates the zendesk reference for member case tickets.
        /// </summary>
        public static string UpdateZenDeskReferenceForOTCRefundOrReshipOrders = "[Orders].[UpdateZenDeskReferenceForOTCRefundOrReshipOrders]";
    }
}
