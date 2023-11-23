using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.Models.ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.Utilities;

namespace ZenDeskTicketProcessJob.SchemaTemplateLayer.Services
{
    public class SchemaTemplateService : ISchemaTemplateService
    {
        public string GetSchemaDefinitionForReimbursementRequestCaseTopic(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string dueDate = DateUtils.GetDateString(caseTickets.DueDate);
            string createdOn = DateUtils.GetDateString(caseTickets.CreateDate);
            string caseCreatedBy = caseTickets.CreateUser;
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";
            string caseTopic = caseTickets.CaseTopic;
            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;

            return $"Case ID#:  {caseID}\n" +
                   $"Due Date: {dueDate}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Case Created By: {caseCreatedBy}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Case Issue: {caseIssue}\n" +
                   $"Card Information: {cardInformation}\n" +
                   $"Case Topic: {caseTopic}\n" +
                   $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}";
        }

        public string GetSchemaDefinitionForChangeCardStatusCaseTopic(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string dueDate = DateUtils.GetDateString(caseTickets.DueDate);
            string createdOn = DateUtils.GetDateString(caseTickets.CreateDate);
            string caseCreatedBy = caseTickets.CreateUser;
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";
            string caseTopic = caseTickets.CaseTopic;

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

            // Access individual properties using TryGetProperty
            string currentStatus, changeStatusTo, reasonForChangingCardStatus;

            if (jsonDocument.RootElement.TryGetProperty("Currentstatus", out var currentStatusElement))
                currentStatus = currentStatusElement.GetString();
            else
                currentStatus = null;

            if (jsonDocument.RootElement.TryGetProperty("ChangeStatusto", out var changeStatusToElement))
                changeStatusTo = changeStatusToElement.GetString();
            else
                changeStatusTo = null;

            if (jsonDocument.RootElement.TryGetProperty("reasonForchangingcardstatus", out var reasonElement))
                reasonForChangingCardStatus = reasonElement.GetString();
            else
                reasonForChangingCardStatus = null;

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;

            // Construct the final message
            return $"{caseID}\n" +
                   $"Due Date: {dueDate}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Case Created By: {caseCreatedBy}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Case Issue: {caseIssue}\n" +
                   $"Card Information: {cardInformation}\n" +
                   $"Case Topic: {caseTopic}\n" +
                   $"Current Status: {currentStatus}\n" +
                   $"Change Status To: {changeStatusTo}\n" +
                   $"Reason for Changing Card Status: {reasonForChangingCardStatus}\n" +
                   $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}";
        }

        public string GetSchemaDefinitionForWalletTransferCaseTopic(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string dueDate = DateUtils.GetDateString(caseTickets.DueDate);
            string createdOn = DateUtils.GetDateString(caseTickets.CreateDate);
            string caseCreatedBy = caseTickets.CreateUser;
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";
            string caseTopic = caseTickets.CaseTopic;

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

            string fromWallet, toWallet, balanceAmount, reasonForMissingFunds;

            if (jsonDocument.RootElement.TryGetProperty("FromWalletValue", out var fromWalletValue))
                fromWallet = fromWalletValue.GetString();
            else
                fromWallet = null;

            if (jsonDocument.RootElement.TryGetProperty("ToWalletValue", out var toWalletValue))
                toWallet = toWalletValue.GetString();
            else
                toWallet = null;

            if (jsonDocument.RootElement.TryGetProperty("BalanceAmount", out var balance))
                balanceAmount = balance.GetString();
            else
                balanceAmount = null;

            if (jsonDocument.RootElement.TryGetProperty("Reason", out var reasonElement))
                reasonForMissingFunds = reasonElement.GetString();
            else
                reasonForMissingFunds = null;

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;

            // Construct the final message
            return $"Case ID#: {caseID}\n" +
                   $"Due Date: {dueDate}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Case Created By: {caseCreatedBy}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Case Issue: {caseIssue}\n" +
                   $"Card Information: {cardInformation}\n" +
                   $"Case Topic: {caseTopic}\n" +
                   $"From Wallet: {fromWallet}\n" +
                   $"To Wallet: {toWallet}\n" +
                   $"Balance Amount: {balanceAmount}\n" +
                   $"Reason for Missing Funds: {reasonForMissingFunds}\n" +
                   $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}";
        }

        public string GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string dueDate = DateUtils.GetDateString(caseTickets.DueDate);
            string createdOn = DateUtils.GetDateString(caseTickets.CreateDate);
            string caseCreatedBy = caseTickets.CreateUser;
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";
            string caseTopic = caseTickets.CaseTopic;

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

            string reasonForCardHolderAddressUpdate = null;
            string newFISAddress = null;

            // Access the "reason" field
            if (jsonDocument.RootElement.TryGetProperty("reason", out var reasonElement))
            {
                // Access the "value" field within "reason"
                if (reasonElement.TryGetProperty("value", out var valueElement))
                {
                    reasonForCardHolderAddressUpdate = valueElement.GetString();
                }
            }

            // Access the "address" field
            if (jsonDocument.RootElement.TryGetProperty("address", out var addressElement))
            {
                // Concatenate values from "firstname" to "zipcode"
                newFISAddress = $"{addressElement.GetProperty("firstname").GetString()} " +
                                $"{addressElement.GetProperty("lastname").GetString()}, " +
                                $"{addressElement.GetProperty("address1").GetString()}, " +
                                $"{addressElement.GetProperty("address2").GetString()}, " +
                                $"{addressElement.GetProperty("city").GetString()}, " +
                                $"{addressElement.GetProperty("state").GetString()}, " +
                                $"{addressElement.GetProperty("stateCode").GetString()}, " +
                                $"{addressElement.GetProperty("zipcode").GetString()}";
            }

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;

            // Construct the final formatted string
            return $"Case ID#:  {caseID}\n" +
                   $"Due Date: {dueDate}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Case Created By: {caseCreatedBy}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Case Issue: {caseIssue}\n" +
                   $"Card Information: {cardInformation}\n" +
                   $"Case Topic: {caseTopic}\n" +
                   $"Reason for Cardholder Address Update: {reasonForCardHolderAddressUpdate}\n" +
                   $"New FIS Address: {newFISAddress}\n" +
                   $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}";
        }

        public string GetSchemaDefinitionForCardReplacementCaseTopic(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string dueDate = DateUtils.GetDateString(caseTickets.DueDate);
            string createdOn = DateUtils.GetDateString(caseTickets.CreateDate);
            string caseCreatedBy = caseTickets.CreateUser;
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";
            string caseTopic = caseTickets.CaseTopic;

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

            string reasonForCardReplacement = null;
            string mailingAddress = null;

            // Access the "reason" field
            if (jsonDocument.RootElement.TryGetProperty("reason", out var reasonElement))
            {
                // Access the "value" field within "reason"
                if (reasonElement.TryGetProperty("value", out var valueElement))
                {
                    reasonForCardReplacement = valueElement.GetString();
                }
            }

            // Access the "address" field
            if (jsonDocument.RootElement.TryGetProperty("address", out var addressElement))
            {
                // Concatenate values from "firstname" to "zipcode"
                mailingAddress = $"{addressElement.GetProperty("firstname").GetString()} " +
                                $"{addressElement.GetProperty("lastname").GetString()}, " +
                                $"{addressElement.GetProperty("address1").GetString()}, " +
                                $"{addressElement.GetProperty("address2").GetString()}, " +
                                $"{addressElement.GetProperty("city").GetString()}, " +
                                $"{addressElement.GetProperty("state").GetString()}, " +
                                $"{addressElement.GetProperty("stateCode").GetString()}, " +
                                $"{addressElement.GetProperty("zipcode").GetString()}";
            }

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;

            // Construct the final formatted string
            return $"Case ID#: {caseID}\n" +
                   $"Due Date: {dueDate}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Case Created By: {caseCreatedBy}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Case Issue: {caseIssue}\n" +
                   $"Card Information: {cardInformation}\n" +
                   $"Case Topic: {caseTopic}\n" +
                   $"Reason for Card Replacement: {reasonForCardReplacement}\n" +
                   $"Mailing Address: {mailingAddress}\n" +
                   $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}";
        }

        public string GetSchemaDefinitionForHearingAidCaseTopic(CaseTickets caseTickets)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets);
        }

        public string GetSchemaDefinitionForOtherCaseTopic(CaseTickets caseTickets)
        {
            string memberIssueID = "#" + caseTickets?.CaseNumber;
            string caseID = $"#{caseTickets?.CaseTopic} - {caseTickets.CaseTicketNumber}";
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;
            string isWrittenResolutionRequested = (bool)(caseTickets?.IsWrittenResolutionRequested) ? "Yes" : "No";

            // Construct the final message
            string finalMessage = $"{memberIssueID}\n" +
                                  $"{caseID}\n" +
                                  $"Case Ticket Status: {caseTicketStatus}\n" +
                                  $"Case Issue: {caseIssue}\n" +
                                  $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                                  $"First Contact Resolution: {firstContactResolution}\n" +
                                  $"Resolution Description: {firstContactResolutionDescription}\n" +
                                  $"Is Written Resolution Requested: {isWrittenResolutionRequested}";

            return finalMessage;
        }


        public string GetSchemaDefinitionForOTCCaseTopic(CaseTickets caseTickets)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets);
        }

        private static string GetMessageForHAAndOTCCaseTopics(CaseTickets caseTickets)
        {
            // Assuming jsonString is your JSON string
            Root root = JsonConvert.DeserializeObject<Root>(caseTickets?.CaseTicketData);

            string memberIssueID = "#" + caseTickets?.CaseNumber;
            string caseID = $"#{caseTickets?.CaseTopic} - {caseTickets.CaseTicketNumber}";
            string caseTicketStatus = caseTickets.CaseTicketStatus;
            string caseIssue = caseTickets?.CaseTopic;

            // Order Information
            string orderInformation = $"{root.Order.OrderId} - " +
                    $"{DateUtils.GetDateString(root.Order.OrderDate)} - " +
                    $"${root.Order.TotalAmount}";

            // HA Item Information
            StringBuilder haItemsInformation = new StringBuilder();
            int totalPriceImpacted = 0;

            foreach (var haItem in root.ItemInfo)
            {
                haItemsInformation.AppendLine($"Item ID: {haItem.ItemId}");
                haItemsInformation.AppendLine($"Total Quantity: {haItem.TotalQuantity}");
                haItemsInformation.AppendLine($"Price: ${haItem.Price}");
                haItemsInformation.AppendLine($"Member Issue: {haItem.Issue?.FirstOrDefault()?.IssueName}");
                haItemsInformation.AppendLine($"Impacted Quantity: {haItem.ImpactedQuantity}");
                haItemsInformation.AppendLine($"Impacted Price: ${haItem.ImpactedPrice}");
                haItemsInformation.AppendLine(); // Separate items

                totalPriceImpacted += haItem.ImpactedPrice;
            }

            string totalPriceImpactedMessage = $"Total Price Impacted: ${totalPriceImpacted}";

            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;
            string isWrittenResolutionRequested = (bool)(caseTickets?.IsWrittenResolutionRequested) ? "Yes" : "No";

            // Construct the final message
            string message = $"{memberIssueID}\n" +
                                  $"{caseID}\n" +
                                  $"Case Ticket Status: {caseTicketStatus}\n" +
                                  $"Case Issue: {caseIssue}\n" +
                                  $"Order Information: {orderInformation}\n" +
                                  $"HA Item Information:\n{haItemsInformation}\n" +
                                  $"{totalPriceImpactedMessage}\n" +
                                  $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                                  $"First Contact Resolution: {firstContactResolution}\n" +
                                  $"Resolution Description: {firstContactResolutionDescription}\n" +
                                  $"Is Written Resolution Requested: {isWrittenResolutionRequested}";

            return message;
        }
    }
}
