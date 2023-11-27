using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenDeskTicketProcessJob.Models;

namespace ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces
{
    public interface ISchemaTemplateService
    {
        public string GetSchemaDefinitionForReimbursementRequestCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForChangeCardStatusCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForWalletTransferCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForProviderIssuesCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForBillingIssuesCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForCardReplacementCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForHearingAidCaseTopic(CaseTickets caseTickets);
        public string GetSchemaDefinitionForOthersCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForOTCCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForCardDeclinedCaseTopic(CaseTickets caseTickets);

        public string GetSchemaDefinitionForFlexIssueCaseTopic(CaseTickets caseTickets);
    }
}
