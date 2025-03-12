using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using MimeKit;

namespace Kharazmi.Mvc.MailServer
{
    internal class NullEmailService : IEmailService, INullInstance
    {
        public void Send(MimeMessage message)
        {
        }

        public Task SendAsync(MimeMessage message)
            => Task.CompletedTask;

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
            => new List<EmailMessage>();

        public Task<List<EmailMessage>> ReceiveEmailAsync(int maxCount = 10)
            => Task.FromResult(new List<EmailMessage>());
    }
}