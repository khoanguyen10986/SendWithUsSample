using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sendwithus;
using SendWithUsLib;

namespace SendWithUsDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IOptions<EmailNotificationConfig> _config;
        private readonly IEmailService _emailService;

        public NotificationController(ILogger<NotificationController> logger, IOptions<EmailNotificationConfig> config, IEmailService emailService)
        {
            _logger = logger;
            _config = config;
            _emailService = emailService;
        }

        //[HttpPost]
        public async Task<IActionResult> Post()
        {
            BatchRequestResponse templateResponse = await _emailService.CreateTemplatesAsync();
            if (!templateResponse.success)
            {
                return StatusCode(500, templateResponse);
            }

            var templateReturn = templateResponse.jsonData.FromJson<List<Template>>();

            var templateData = new List<TemplateData>();
            for (int i = 0; i < templateReturn.Count; i++)
            {
                var template = new TemplateData
                {
                    TemplateId = templateReturn[i].id,
                    Data =
                    new Dictionary<string, object>
                    {
                        { "store_name", $"Grocery {i+1}" },
                        { "store_address", $"Address {i+1}" }
                    }
                };
                templateData.Add(template);
            }

            var emailSends = new List<EmailSend>();
            foreach (var template in templateData)
            {
                var emailSend = new EmailSend
                {
                    TemplateData = template,
                    Recipients = new Collection<EmailRecipient> 
                    {
                        new EmailRecipient(_config.Value.RecipientEmails[0]),
                        new EmailRecipient(_config.Value.RecipientEmails[1]),
                    },
                    Bccs = new Collection<EmailRecipient>
                    {
                        new EmailRecipient(_config.Value.RecipientEmails[2])
                    }
                };
                emailSends.Add(emailSend);
            }
            BatchRequestResponse emailResponse = await _emailService.SendEmailsAsync(emailSends);
            if (!emailResponse.success)
            {
                return StatusCode(500, emailResponse);
            }

            return Ok(emailResponse);
        }
    }
}
