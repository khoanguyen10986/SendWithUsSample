using Sendwithus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendWithUsLib
{
    public interface IEmailService
    {
        Task<BatchRequestResponse> CreateTemplatesAsync();
        Task<BatchRequestResponse> SendEmailsAsync(List<EmailSend> emailSend);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSetting _emailSetting;

        public EmailService()
        {
            _emailSetting = EmailHelper.ToEmailSetting();
            SendwithusClient.ApiKey = _emailSetting.EmailConfig.ApiKey;
            SendwithusClient.SetTimeoutInMilliseconds(_emailSetting.EmailConfig.TimeoutInMilliseconds);
            SendwithusClient.RetryCount = _emailSetting.EmailConfig.RetryCount;
            SendwithusClient.RetryIntervalMilliseconds = _emailSetting.EmailConfig.RetryIntervalMilliseconds;
        }

        public async Task<BatchRequestResponse> CreateTemplatesAsync()
        {
            BatchRequestResponse response = await SendBatchRequestAsync(async () =>
            {

                var templateVersion1 = new TemplateVersion
                {
                    name = $"Store 1",
                    subject = $"Store 1",
                    html = "Template1.html".ToHtmlTemplate(),
                };
                await Template.CreateTemplateAsync(templateVersion1);

                var templateVersion2 = new TemplateVersion
                {
                    name = $"Store 2",
                    subject = $"Store 2",
                    html = "Template2.html".ToHtmlTemplate(),
                };
                await Template.CreateTemplateAsync(templateVersion2);
            });

            return response;
        }

        public async Task<BatchRequestResponse> SendEmailsAsync(List<EmailSend> emailSends)
        {
            try
            {
                var emailTasks = new List<Task<EmailResponse>>();

                foreach (var emailSend in emailSends)
                {
                    var tasks = emailSend.Recipients.Select(recipient =>
                    {
                        var email = new Email(emailSend.TemplateData.TemplateId, emailSend.TemplateData.Data, recipient);
                        foreach (var bcc in emailSend.Bccs)
                        {
                            email.bcc.Add(bcc);
                        }
                        return email.Send();
                    });

                    emailTasks.AddRange(tasks);
                }

                EmailResponse[] response = await Task.WhenAll(emailTasks);

                return new BatchRequestResponse(true, "200", response.ToJson());
            }
            catch (AggregateException ex)
            {
                return new BatchRequestResponse(false, "500", null, ex.ToString());
            }
        }

        private async Task<BatchRequestResponse> SendBatchRequestAsync(Func<Task> executeRequests)
        {
            BatchApiRequest.StartNewBatchRequest();

            try
            {
                bool isBatchSuccess = true;
                var data = new List<object>();

                await executeRequests();

                List<BatchApiResponse> batchResponses = await BatchApiRequest.SendBatchApiRequest();
                for (int i = 0; i < batchResponses.Count; i++)
                {
                    BatchApiResponse batchResponse = batchResponses[i];
                    if (batchResponse.status_code != 200)
                    {
                        isBatchSuccess = false;
                        BatchApiRequest.AbortBatchRequest();
                        break;
                    }
                    var body = batchResponses[i].GetBody<object>();
                    data.Add(body);
                }

                var response = new BatchRequestResponse(isBatchSuccess, "200", data.ToJson());
                return response;
            }
            catch (AggregateException ex)
            {
                BatchApiRequest.AbortBatchRequest();
                return new BatchRequestResponse(false, "500", null, ex.ToString());
            }
            catch (InvalidOperationException ex)
            {
                BatchApiRequest.AbortBatchRequest();
                return new BatchRequestResponse(false, "500", null, ex.ToString());
            }
        }
    }
}
