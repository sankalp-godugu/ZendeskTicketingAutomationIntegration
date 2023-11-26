using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.Models.ZenDeskTicketProcessJob.Models;
using ZenDeskTicketProcessJob.SchemaTemplateLayer.Interfaces;
using ZenDeskTicketProcessJob.Utilities;

namespace ZenDeskTicketProcessJob.SchemaTemplateLayer.Services
{
    /// <summary>
    /// Schema template service.
    /// </summary>
    public class SchemaTemplateService : ISchemaTemplateService
    {
        /// <summary>
        /// Gets the schema for the reimbursement request case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForReimbursementRequestCaseTopic(CaseTickets caseTickets)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);
            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final message
            return $"{commonMessage}{resolutionMessage}";
        }

        /// <summary>
        /// Gets the schema for the change card status case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForChangeCardStatusCaseTopic(CaseTickets caseTickets)
        {
            // Common message.
            string commonMessage = ConstructCommonMessage(caseTickets);

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

            // Resolution message.
            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final message
            return commonMessage +
                   $"Current Status: {currentStatus}\n" +
                   $"Change Status To: {changeStatusTo}\n" +
                   $"Reason for Changing Card Status: {reasonForChangingCardStatus}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the billing issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForBillingIssuesCaseTopic(CaseTickets caseTickets)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);

            // Assuming jsonString is your JSON string
            Root root = JsonConvert.DeserializeObject<Root>(caseTickets?.CaseTicketData);

            // Gets the order information.
            string orderInformation = GetOrderInformation(root);

            // Is benefit applied.
            bool wasBenefitApplied = root.Order.BenefitApplied;

            string resolutionMessage = ConstructResolutionMessage(caseTickets);
            return commonMessage +
                   $"Order Information: {orderInformation}\n" +
                   $"Was Benefit Applied: {wasBenefitApplied}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the shipment related issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(CaseTickets caseTickets)
        {
           return GetMessageForHAAndOTCCaseTopics(caseTickets);
        }

        /// <summary>
        /// Gets the schema for the billing issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForProviderIssuesCaseTopic(CaseTickets caseTickets)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);

            // Deserialize the JSON string
            var jsonObject = JsonConvert.DeserializeObject<JObject>(caseTickets.CaseTicketData);

            // Extract values
            string memberAppointmentId = jsonObject["appointment"]["memberAppointmentId"].ToString();
            string providerName = jsonObject["appointment"]["providerName"].ToString();
            string providerLocation = jsonObject["appointment"]["providerLocation"].ToString();
            string hcpName = jsonObject["appointment"]["hcpName"].ToString();

            // Deserialize appointmentProcessData
            var appointmentProcessData = JsonConvert.DeserializeObject<JObject>(jsonObject["appointment"]["appointmentProcessData"].ToString());
            string dateOfService = DateUtils.GetDateString(Convert.ToDateTime(appointmentProcessData["DateOfService"]));


            string resolutionMessage = ConstructResolutionMessage(caseTickets);
            return commonMessage +
                   $"Appointment ID: #{memberAppointmentId}\n" +
                   $"Provider Name: {providerName}\n" +
                   $"Provider Location: {providerLocation}\n" +
                   $"HCP Name: {hcpName}\n" +
                   $"Date of Interaction: {dateOfService}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the wallet transfer case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForWalletTransferCaseTopic(CaseTickets caseTickets)
        {
            // Common message.
            string commonMessage = ConstructCommonMessage(caseTickets);

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
                balanceAmount = balance.GetRawText();
            else
                balanceAmount = null;

            if (jsonDocument.RootElement.TryGetProperty("Reason", out var reasonElement))
                reasonForMissingFunds = reasonElement.GetString();
            else
                reasonForMissingFunds = null;

            // Resolution message.
            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final message
            return commonMessage +
                   $"From Wallet: {fromWallet}\n" +
                   $"To Wallet: {toWallet}\n" +
                   $"Balance Amount: {balanceAmount}\n" +
                   $"Reason for Missing Funds: {reasonForMissingFunds}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the cardholder address update case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(CaseTickets caseTickets)
        {
            // Common message.
            string commonMessage = ConstructCommonMessage(caseTickets);

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

            // Resolution message.
            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final formatted string
            return commonMessage +
                   $"Reason for Cardholder Address Update: {reasonForCardHolderAddressUpdate}\n" +
                   $"New FIS Address: {newFISAddress}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the card replacement case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForCardReplacementCaseTopic(CaseTickets caseTickets)
        {
            // Common message.
            string commonMessage = ConstructCommonMessage(caseTickets);

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

            // Resolution message.
            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final formatted string
            return commonMessage +
                   $"Reason for Card Replacement: {reasonForCardReplacement}\n" +
                   $"Mailing Address: {mailingAddress}\n" +
                   resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the hearing id  case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForHearingAidCaseTopic(CaseTickets caseTickets)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets);
        }

        /// <summary>
        /// Gets the schema for the other case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForOthersCaseTopic(CaseTickets caseTickets)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);
            string resolutionMessage = ConstructResolutionMessage(caseTickets);
            return commonMessage + resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the OTC case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForOTCCaseTopic(CaseTickets caseTickets)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets);
        }

        #region Private Methods

        /// <summary>
        /// Gets the message for HA and OTC case topics.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        private string GetMessageForHAAndOTCCaseTopics(CaseTickets caseTickets)
        {
            // Gets the common message.
            string commonMessage = ConstructCommonMessage(caseTickets);

            // Assuming jsonString is your JSON string
            Root root = JsonConvert.DeserializeObject<Root>(caseTickets?.CaseTicketData);

            // Gets the order information.
            string orderInformation = GetOrderInformation(root);

            // HA Item Information
            StringBuilder haItemsInformation = new StringBuilder();
            decimal totalPriceImpacted = 0;
            string totalPriceImpactedMessage = string.Empty;

            if (root.ItemInfo != null && root.ItemInfo.Count > 0)
            {
                foreach (var haItem in root.ItemInfo)
                {
                    haItemsInformation.AppendLine($"Item ID: {haItem?.ItemId}");
                    haItemsInformation.AppendLine($"Total Quantity: {haItem?.TotalQuantity}");
                    haItemsInformation.AppendLine($"Price: ${haItem?.Price}");
                    haItemsInformation.AppendLine($"Member Issue: {haItem.Issue?.FirstOrDefault()?.IssueName}");
                    haItemsInformation.AppendLine($"Impacted Quantity: {haItem?.ImpactedQuantity}");
                    haItemsInformation.AppendLine($"Impacted Price: ${haItem?.ImpactedPrice}");
                    haItemsInformation.AppendLine();

                    totalPriceImpacted += haItem.ImpactedPrice;
                }
                totalPriceImpactedMessage = $"Total Price Impacted: ${totalPriceImpacted}";
            }

            string resolutionMessage = ConstructResolutionMessage(caseTickets);

            // Construct the final message
            string message = commonMessage +
                                  $"Order Information: {orderInformation}\n" +
                                  $"HA Item Information:\n{haItemsInformation}\n" +
                                  $"{totalPriceImpactedMessage}\n" +
                                  resolutionMessage;

            return message;
        }

        /// <summary>
        /// Constructs the common message for all case topics.
        /// </summary>
        /// <param name="caseTickets">Case tickets.</param>
        /// <returns>Returns the common message.</returns>
        private string ConstructCommonMessage(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets?.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string createdBy = caseTickets?.CreateUserFullName;
            string createdOn = DateUtils.GetDateString(caseTickets?.CreateDate);
            string issueGenre = caseTickets?.CaseCategory;
            string issueType = caseTickets?.CaseType;
            string issueTopic = caseTickets?.CaseTopic;
            string assignedTo = caseTickets?.AssignToFullName;
            string caseTicketStatus = caseTickets?.CaseTicketStatus;
            string cardInformation = $"XXXX-XXXX-XXXX-{caseTickets?.CardLast4digits}";

            // Construct the common message
            return $"Case ID#: {caseID}\n" +
                   $"Created By: {createdBy}\n" +
                   $"Created On: {createdOn}\n" +
                   $"Issue Genre: {issueGenre}\n" +
                   $"Issue Type: {issueType}\n" +
                   $"Due Date: {issueTopic}\n" +
                   $"Assigned To: {assignedTo}\n" +
                   $"Case Ticket Status: {caseTicketStatus}\n" +
                   $"Card Information: {cardInformation}\n";
        }

        /// <summary>
        /// Constructs the resolution message for all case tickets.
        /// </summary>
        /// <param name="caseTickets">Case tickets.</param>
        /// <returns>Returns the resolution message.</returns>
        private string ConstructResolutionMessage(CaseTickets caseTickets)
        {
            string additionalDetailsOrActionTaken = caseTickets?.AdditionalInfo;
            string descriptionOfComplaint = GetDescriptionOfComplaint(caseTickets);
            string firstContactResolution = (bool)caseTickets?.IsFirstCallResolution ? "Yes" : "No";
            string firstContactResolutionDescription = caseTickets?.FirstCallResolutionDesc;
            string isWrittenResolutionRequested = (bool)(caseTickets?.IsWrittenResolutionRequested) ? "Yes" : "No";

            // Construct the resolution message
            return $"Additional Details or Action Taken: {additionalDetailsOrActionTaken}\n" +
                   $"Description of Complaint : {descriptionOfComplaint}\n" +
                   $"First Contact Resolution: {firstContactResolution}\n" +
                   $"Resolution Description: {firstContactResolutionDescription}\n" +
                   $"Is Written Resolution Requested: {isWrittenResolutionRequested}\n";
        }

        /// <summary>
        /// Gets the description of complaint.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the desciption of complaint.</returns>
        private string GetDescriptionOfComplaint(CaseTickets caseTickets)
        {
            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

            string additionalInfoValue = null;

            // Access the "additinalinfro" field
            if (jsonDocument.RootElement.TryGetProperty("additinalinfro", out var additionalInfoElement))
            {
                additionalInfoValue = additionalInfoElement.GetString();
            }

            // Now 'additionalInfoValue' contains the value inside "additinalinfro"
            return additionalInfoValue;
        }

        /// <summary>
        /// Gets the order information from the case tickets.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the order information from the case ticket.</returns>
        private string GetOrderInformation(Root root)
        {
            string orderInformation = string.Empty;
            if (root.Order != null)
            {
                // Order Information
                orderInformation = $"{root?.Order?.OrderId} - " +
                        $"{DateUtils.GetDateString(root?.Order?.OrderDate)} - " +
                        $"${root?.Order?.TotalAmount}";
            }
            return orderInformation;
        }

        #endregion

    }
}
