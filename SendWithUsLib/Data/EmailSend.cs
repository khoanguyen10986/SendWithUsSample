using Sendwithus;
using System.Collections.ObjectModel;

namespace SendWithUsLib
{
    public class EmailSend
    {
        public TemplateData TemplateData { get; set; }
        public Collection<EmailRecipient> Recipients { get; set; }
        public Collection<EmailRecipient> Bccs { get; set; }

        public EmailSend()
        {
            Recipients = new Collection<EmailRecipient>();
            Bccs = new Collection<EmailRecipient>();
        }
    }
}
