#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using MimeKit;

#endregion

namespace Kharazmi.Mvc.MailServer
{
    public interface IEmailService: IShouldBeSingleton, IMustBeInstance
    {
        void Send(MimeMessage message);
        Task SendAsync(MimeMessage message);

        List<EmailMessage> ReceiveEmail(int maxCount = 10);
        Task<List<EmailMessage>> ReceiveEmailAsync(int maxCount = 10);
    }
}