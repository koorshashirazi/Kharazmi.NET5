#region

using System.Collections.Generic;

#endregion

namespace Kharazmi.Mvc.MailServer
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            Receivers = new List<MailAddress>();
            Senders = new List<MailAddress>();
        }

        public List<MailAddress> Receivers { get; set; }
        public List<MailAddress> Senders { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}