using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pelorus.Common;
using Pelorus.Common.Enums;
using Pelorus.Common.Helpers;
using Pelorus.Common.Models;
using Pelorus.MicroService.AdConnection.Managers;
using Pelorus.Service.Notifications.Interface;
using Sendwithus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace Pelorus.Service.Notifications.Email
{
    public class EmailMessagingService : INotifyService
    {
        private EmailSettings _emailSettings = null;
        public EmailMessagingService()
        {            
            _emailSettings = SettingsHelper.LoadSettings<EmailSettings>("emailSettings", "EmailSettings");

            SendwithusClient.ApiKey = _emailSettings.ApiKey;            
            SendwithusClient.SetTimeoutInMilliseconds(_emailSettings.Timeout);
            SendwithusClient.RetryCount = _emailSettings.RetryCount;
            SendwithusClient.RetryIntervalMilliseconds = _emailSettings.RetryInterval;
        }      

        public async void SendOrderSubmittedMessageAsync(Order submittedOrder, string organisationId)
        {
            var responses = await SendMessage(submittedOrder, organisationId);            
        }

        private Dictionary<string, object> CreateTemplateData(Order order)
        {            
            Dictionary<string, object> templateData = null;
            if (order != null)
            {
                templateData = new Dictionary<string, object>();
                var submittedEvent = order.Events?.FirstOrDefault(x => x.Status == OrderStatus.OrderSubmitted);
                var confirmedTransportEvent = order.Events?.FirstOrDefault(x => x.Status == OrderStatus.OrderConfirmedForTransport);
                var completedEvent = order.Events?.FirstOrDefault(x => x.Status == OrderStatus.OrderCompleted);

                templateData.Add("OrderId", order.OrderId);
                templateData.Add("Reference", order.OrderProperties?.Reference);
                templateData.Add("SubmittedBy", submittedEvent?.CreatedBy);
              
                // To do: should get supplier name from app settings or DB 
                templateData.Add("SubmittedByOrg", order?.OrderProperties?.Supplier?.Name ?? "Submitted by value not loaded from order properties");
                templateData.Add("CollectionAddress", order.OrderValues.CollectionAddress);
                templateData.Add("DestinationAddress", order.OrderValues.DestinationAddress);
                templateData.Add("CollectionDate", order.OrderValues.CollectionDate?.AddDays(1).ToString(Constants.DateTimeFormats.CollectionFormat));
                templateData.Add("NumberOfPallets", order.OrderValues.Products?.Sum(item => item.NumberOfPallets));
                templateData.Add("Comment", order.OrderValues.Comment);
                templateData.Add("ConfirmedBy", confirmedTransportEvent?.CreatedBy);

                // To do: should get warehouse name from app settings or DB 
                templateData.Add("ConfirmedByOrg", GraphApiSettingsHelpers.Instance.QCOrganisationName);
                templateData.Add("CompletedBy", completedEvent?.CreatedBy);
                templateData.Add("CompletedByOrg", GraphApiSettingsHelpers.Instance.QCOrganisationName);
            }
            return templateData;
        }        
        
        public async Task<List<EmailResponse>> SendMessage(Order order, string organisationId)
        {
            List<EmailResponse> responses = new List<EmailResponse>();
            var orderStatus = EnumHelpers.GetDescriptionFromValue(order.OrderStatus);
            var orderMessage = _emailSettings.OrderMessages.FirstOrDefault(x => string.Equals(x.OrderStatus, orderStatus, StringComparison.InvariantCultureIgnoreCase));
            if (orderMessage != null)
            {
                var organisation = orderMessage.Organisations.FirstOrDefault(x => x.OrganisationId == organisationId);
                var templateId = organisation.TemplateId;
                var users = await GroupMembersManager.Instance.Members(organisationId);
                var recipients = users?.Select(x => new EmailRecipient(x.Mail,x.GivenName)).ToList() ?? null;
                var templateData = CreateTemplateData(order);
                responses = await SendMessage(templateId, templateData, recipients);
            }
            return responses;
        }

        private async Task<List<EmailResponse>> SendMessage(string templateId, Dictionary<string, object> templateData, List<EmailRecipient> recipients)
        {
            List<EmailResponse> responses = new List<EmailResponse>();
            var templateString = JsonConvert.SerializeObject(templateData);
            if (!string.IsNullOrWhiteSpace(templateId) && templateData != null && recipients != null)
            {
                foreach (var recipient in recipients)
                {
                    var personalisedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(templateString);
                    personalisedData.Add("Recipient", recipient.name);
                    var email = new Sendwithus.Email(templateId, personalisedData, recipient);
                    try
                    {
                        var emailResponse = await email.Send();
                        responses.Add(emailResponse);
                    }
                    catch (AggregateException ex)
                    {
                        responses.Add(new EmailResponse()
                        {
                            success = false,
                            status = ex.Message
                        });
                    }
                }
            }
            return responses;
        }

        public async void SendOrderConfirmedForTransportMessageAsync(Order order, string organisationId)
        {
            var responses = await SendMessage(order, organisationId);
        }

        public async void SendTruckAssignedMessageAsync(Order order, string organisationId)
        {
            var responses = await SendMessage(order, organisationId);
        }

        public async void SendGoodsPickedUpMessageAsync(Order order, string organisationId)
        {
            var responses = await SendMessage(order, organisationId);
        }

        public async void SendGoodsArrivedMessageAsync(Order order, string organisationId)
        {
            var responses = await SendMessage(order, organisationId);
        }

        public async void SendOrderCompletedMessageAsync(Order order, string organisationId)
        {
            var responses = await SendMessage(order, organisationId);
        }
    }
}
