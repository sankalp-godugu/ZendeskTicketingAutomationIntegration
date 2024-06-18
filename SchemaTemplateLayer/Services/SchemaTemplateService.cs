using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
    /// <summary>
    /// Schema template service.
    /// </summary>
    public class SchemaTemplateService : ISchemaTemplateService
    {
        /// <summary>
        /// Gets the schema for the reimbursement request case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param> 
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForReimbursementRequestCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);
            string resolutionMessage = ConstructResolutionMessage(caseTickets);
            return $"{commonMessage}{resolutionMessage}";
        }

        /// <summary>
        /// Gets the schema for the change card status case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForChangeCardStatusCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                // Common message.
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Parse JSON string to JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

                // Access individual properties using TryGetProperty
                string currentStatus = GetPropertyValue(jsonDocument, "Currentstatus");
                string changeStatusTo = GetPropertyValue(jsonDocument, "ChangeStatusto");
                string reasonForChangingCardStatus = GetPropertyValue(jsonDocument, "reasonForchangingcardstatus");

                // Resolution message.
                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                // Construct the final message
                return commonMessage +
                       $"Current Status: {currentStatus}\n" +
                       $"Change Status To: {changeStatusTo}\n" +
                       $"Reason for Changing Card Status: {reasonForChangingCardStatus}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger.LogError($"JSON Exception occured for getting the schema defintion for change card status case topic: {jsonEx}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception occured for getting the schema defintion for change card status case topic: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the schema for the billing issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForBillingIssuesCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Assuming jsonString is your JSON string
                Root root = JsonConvert.DeserializeObject<Root>(caseTickets?.CaseTicketData);

                // Gets the order information.
                string orderInformation = GetOrderInformation(root);

                // Is benefit applied.
                string wasBenefitApplied = root.Order != null ? root.Order.BenefitApplied ? "Yes" : "No" : "Not found";

                string resolutionMessage = ConstructResolutionMessage(caseTickets);
                return commonMessage +
                       $"Order Information: {orderInformation}\n" +
                       $"Was Benefit Applied: {wasBenefitApplied}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for billing issues case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for billing issues case topic: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the schema for the shipment related issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForShipmentRelatedIssuesCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets, logger);
        }

        /// <summary>
        /// Gets the schema for the provider issues case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in json.</returns>
        public string GetSchemaDefinitionForProviderIssuesCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Deserialize the JSON string
                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(caseTickets.CaseTicketData);

                // Extract values
                string memberAppointmentId = jsonObject?["appointment"]?["memberAppointmentId"]?.ToString();
                string providerName = jsonObject?["appointment"]?["providerName"]?.ToString();
                string providerLocation = jsonObject?["appointment"]?["providerLocation"]?.ToString();
                string hcpName = jsonObject?["appointment"]?["hcpName"]?.ToString();

                // Deserialize appointmentProcessData
                JObject appointmentProcessData = JsonConvert.DeserializeObject<JObject>(jsonObject?["appointment"]?["appointmentProcessData"]?.ToString());
                string dateOfService = DateUtils.GetDateString(Convert.ToDateTime(appointmentProcessData?["DateOfService"]));

                string resolutionMessage = ConstructResolutionMessage(caseTickets);
                return commonMessage +
                       $"Appointment ID: #{memberAppointmentId}\n" +
                       $"Provider Name: {providerName}\n" +
                       $"Provider Location: {providerLocation}\n" +
                       $"HCP Name: {hcpName}\n" +
                       $"Date of Interaction: {dateOfService}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for provider issues case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for provider issues case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the card declined case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in json.</returns>
        public string GetSchemaDefinitionForCardDeclinedCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                string commonMessage = ConstructCommonMessage(caseTickets);

                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(caseTickets.CaseTicketData);

                // Accessing TransactionDate, TransactionDetails, and Reason
                string transactionDate = jsonObject?["TransactionDate"]?.ToString();
                string transactionDetails = jsonObject?["TransactionDetails"]?.ToString();
                string reason = jsonObject?["Reason"]?.ToString();

                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                return commonMessage +
                       $"Transaction Date: {DateUtils.GetDateString(Convert.ToDateTime(transactionDate))}\n" +
                       $"Transaction Details: {transactionDetails}\n" +
                       $"Reason for: {reason}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for card declined case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for card declined case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the flex issue case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in json.</returns>
        public string GetSchemaDefinitionForFlexIssueCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                string commonMessage = ConstructCommonMessage(caseTickets);

                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(caseTickets.CaseTicketData);

                // Accessing TransactionDate, TransactionDetails, and Reason
                string fromWalletValue = jsonObject?["FromWalletValue"]?.ToString();
                string toWalletValue = jsonObject?["ToWalletValue"]?.ToString();
                double balanceAmount = jsonObject?["BalanceAmount"]?.Value<double>() ?? 0.0; // Assuming BalanceAmount is a numeric value
                string reason = jsonObject?["Reason"]?.ToString();

                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                return commonMessage +
                       $"Wallet Transfer Reason: Transaction amount of ${balanceAmount} from {fromWalletValue} to {toWalletValue} wallet with reason {reason}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for flex issue case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for flex issue case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the wallet transfer case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in JSON.</returns>
        public string GetSchemaDefinitionForWalletTransferCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                // Common message.
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Parse JSON string to JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

                // Access individual properties using the GetPropertyValue method
                string fromWallet = GetPropertyValue(jsonDocument, "FromWalletValue");
                string toWallet = GetPropertyValue(jsonDocument, "ToWalletValue");
                string balanceAmount = GetPropertyValue(jsonDocument, "BalanceAmount");
                string reasonForMissingFunds = GetPropertyValue(jsonDocument, "Reason");

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
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for wallet transfer case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for wallet transfer case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the cardholder address update case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in JSON.</returns>
        public string GetSchemaDefinitionForCardholderAddressUpdateCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                // Common message.
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Parse JSON string to JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

                // Access individual properties using the GetPropertyValue method
                string reasonForCardHolderAddressUpdate = GetPropertyValue(jsonDocument, "reason.value");
                // Build the newFISAddress string without adding empty components
                List<string> addressComponents = new()
                {
                    GetPropertyValue(jsonDocument, "address.firstname"),
                    GetPropertyValue(jsonDocument, "address.lastname"),
                    GetPropertyValue(jsonDocument, "address.address1"),
                    GetPropertyValue(jsonDocument, "address.address2"),
                    GetPropertyValue(jsonDocument, "address.city"),
                    GetPropertyValue(jsonDocument, "address.state"),
                    GetPropertyValue(jsonDocument, "address.stateCode"),
                    GetPropertyValue(jsonDocument, "address.zipcode")
                };

                // Filter out empty components and concatenate them
                string newFISAddress = string.Join(", ", addressComponents.Where(component => !string.IsNullOrEmpty(component)));

                // Resolution message.
                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                // Construct the final formatted string
                return commonMessage +
                       $"Reason for Cardholder Address Update: {reasonForCardHolderAddressUpdate}\n" +
                       $"New FIS Address: {newFISAddress}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for cardholder address update case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for cardholder address update case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the card replacement case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in JSON.</returns>
        public string GetSchemaDefinitionForCardReplacementCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                // Common message.
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Parse JSON string to JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(caseTickets?.CaseTicketData);

                // Access individual properties using the GetPropertyValue method
                string reasonForCardReplacement = GetPropertyValue(jsonDocument, "reason.value");
                // Build the mailingAddress string without adding empty components
                List<string> mailingAddressComponents = new()
                {
                    GetPropertyValue(jsonDocument, "address.firstname"),
                    GetPropertyValue(jsonDocument, "address.lastname"),
                    GetPropertyValue(jsonDocument, "address.address1"),
                    GetPropertyValue(jsonDocument, "address.address2"),
                    GetPropertyValue(jsonDocument, "address.city"),
                    GetPropertyValue(jsonDocument, "address.state"),
                    GetPropertyValue(jsonDocument, "address.stateCode"),
                    GetPropertyValue(jsonDocument, "address.zipcode")
                };

                // Filter out empty components and concatenate them
                string mailingAddress = string.Join(", ", mailingAddressComponents.Where(component => !string.IsNullOrEmpty(component)));


                // Resolution message.
                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                // Construct the final formatted string
                return commonMessage +
                       $"Reason for Card Replacement: {reasonForCardReplacement}\n" +
                       $"Mailing Address: {mailingAddress}\n" +
                       resolutionMessage;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for card replacement case topic: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for card replacement case topic: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the schema for the hearing id  case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForHearingAidCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets, logger);
        }

        /// <summary>
        /// Gets the schema for the other case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForOthersCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            string commonMessage = ConstructCommonMessage(caseTickets);
            string resolutionMessage = ConstructResolutionMessage(caseTickets);
            return commonMessage + resolutionMessage;
        }

        /// <summary>
        /// Gets the schema for the OTC case topic.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the schema defintion in json.</returns>
        public string GetSchemaDefinitionForOTCCaseTopic(CaseTickets caseTickets, ILogger logger)
        {
            return GetMessageForHAAndOTCCaseTopics(caseTickets, logger);
        }


        #region Private Methods

        /// <summary>
        /// Gets the message for HA and OTC case topics.
        /// </summary>
        /// <param name="caseTickets">Case tickets.<see cref="CaseTickets"/></param>
        /// <param name="logger">Logger for logging.</param>
        /// <returns>Returns the schema definition in json.</returns>
        private string GetMessageForHAAndOTCCaseTopics(CaseTickets caseTickets, ILogger logger)
        {
            try
            {
                // Gets the common message.
                string commonMessage = ConstructCommonMessage(caseTickets);

                // Assuming jsonString is your JSON string
                Root root = JsonConvert.DeserializeObject<Root>(caseTickets?.CaseTicketData);

                // Gets the order information.
                string orderInformation = GetOrderInformation(root);

                // HA Item Information
                StringBuilder haItemsInformation = new();
                decimal totalPriceImpacted = 0;
                string totalPriceImpactedMessage = string.Empty;

                if (root.ItemInfo != null && root.ItemInfo.Count > 0)
                {
                    foreach (ItemInfo haItem in root.ItemInfo)
                    {
                        _ = haItemsInformation.AppendLine($"Item ID: {haItem?.ItemId}");
                        _ = haItemsInformation.AppendLine($"Total Quantity: {haItem?.TotalQuantity}");
                        _ = haItemsInformation.AppendLine($"Price: ${haItem?.Price}");
                        _ = haItemsInformation.AppendLine($"Member Issue: {haItem.Issue?.FirstOrDefault()?.IssueName}");
                        _ = haItemsInformation.AppendLine($"Impacted Quantity: {haItem?.ImpactedQuantity}");
                        _ = haItemsInformation.AppendLine($"Impacted Price: ${haItem?.ImpactedPrice}");
                        _ = haItemsInformation.AppendLine();

                        totalPriceImpacted += haItem.ImpactedPrice;
                    }
                    totalPriceImpactedMessage = $"Total Price Impacted: ${totalPriceImpacted}";
                }

                string resolutionMessage = ConstructResolutionMessage(caseTickets);

                // Construct the final message
                string message = commonMessage +
                    $"Order Information: {orderInformation}\n" +
                    (root.ItemInfo != null && root.ItemInfo.Count > 0 ? $"Item Information:\n{haItemsInformation}\n" : string.Empty) +
                    $"{totalPriceImpactedMessage}\n" +
                    resolutionMessage;

                return message;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                logger?.LogError($"Error parsing JSON for HA and OTC case topics: {jsonEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError($"An error occurred for HA and OTC case topics: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Constructs the common message for all case topics.
        /// </summary>
        /// <param name="caseTickets">Case tickets.</param>
        /// <returns>Returns the common message.</returns>
        private string ConstructCommonMessage(CaseTickets caseTickets)
        {
            string caseID = $"{caseTickets?.CaseTicketNumber} - {caseTickets?.CaseTopic}";
            string createdBy = caseTickets?.CreateUser;
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
                   $"Assigned To: {caseTickets?.AssignedTo}\n" +
                   $"Issue Genre: {issueGenre}\n" +
                   $"Issue Type: {issueType}\n" +
                   $"Issue Topic: {issueTopic}\n" +
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
            if (jsonDocument.RootElement.TryGetProperty("additinalinfro", out JsonElement additionalInfoElement))
            {
                additionalInfoValue = additionalInfoElement.GetString();
            }

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

        /// <summary>
        /// Gets the property value from the JSON document for the passed property name.
        /// </summary>
        /// <param name="jsonDocument">JSON document.<see cref="JsonDocument"/></param>
        /// <param name="propertyName">Property name.<see cref="propertyName"/></param>
        /// <returns>Returns the value of the property as a string, or null if not found.</returns>
        private string GetPropertyValue(JsonDocument jsonDocument, string propertyPath)
        {
            JsonElement element = jsonDocument.RootElement;

            foreach (string property in propertyPath.Split('.'))
            {
                if (element.TryGetProperty(property, out JsonElement nextElement))
                {
                    element = nextElement;
                }
                else
                {
                    // Handle the case where the property doesn't exist
                    return null;
                }
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True or JsonValueKind.False => element.GetBoolean().ToString(),
                _ => element.ToString(),
            };
        }


        #endregion

    }
}
