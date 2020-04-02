using Sendwithus;
using System.Collections.ObjectModel;

namespace SendWithUsLib
{
    public class EmailSend
    {
        public TemplateData TemplateData { get; set; }
        public EmailRecipient Recipient { get; set; }
        public Collection<EmailRecipient> Ccs { get; set; }
        public Collection<EmailRecipient> Bccs { get; set; }

        public EmailSend()
        {
            Ccs = new Collection<EmailRecipient>();
            Bccs = new Collection<EmailRecipient>();
        }
    }
}
