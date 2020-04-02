using System;
using System.Collections.Generic;
using System.Text;

namespace Pelorus.Service.Notifications.Email
{
    public class EmailSettings
    {
        public string ApiKey { get; set; }
        public int RetryCount { get; set; }
        public int Timeout { get; set; }
        public int RetryInterval { get; set; }
        public List<OrderMessage> OrderMessages { get; set; }
    }

    public class OrderMessage
    {
        public string OrderStatus { get; set; }        
        public List<Organisation> Organisations { get; set; }
    }

    public class Organisation
    {
        public string OrganisationId { get; set; }
        public string TemplateId { get; set; }
        public List<string> EmailRecipients { get; set; }
    }
}
