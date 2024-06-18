using Microsoft.Extensions.Logging;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces
{
    public interface ISchemaTemplateService
    {
        public string GetSchemaDefinitionForReimbursementRequestCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForChangeCardStatusCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForWalletTransferCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForProviderIssuesCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForBillingIssuesCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForCardReplacementCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForHearingAidCaseTopic(CaseTickets caseTickets, ILogger logger);
        public string GetSchemaDefinitionForOthersCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForOTCCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForCardDeclinedCaseTopic(CaseTickets caseTickets, ILogger logger);

        public string GetSchemaDefinitionForFlexIssueCaseTopic(CaseTickets caseTickets, ILogger logger);
    }
}
