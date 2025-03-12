#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Kharazmi.Threading;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MimeKit;

#endregion

namespace Kharazmi.Mvc.MailServer
{
    /// <summary> Email server service </summary>
    internal class EmailService : IEmailService
    {
        private readonly EmailServerOption _serverOption;

        public EmailService(ISettingProvider settingProvider)
            => _serverOption = settingProvider.Get<EmailServerOption>();

        public void Send(MimeMessage message)
            => AsyncHelper.RunSync(() => SendAsync(message));


        /// <summary>
        /// Sends an email using the `MailKit` library.
        /// </summary>
        public async Task SendAsync(MimeMessage message)
        {
            if (_serverOption.UsePickupFolder)
            {
                const int maxBufferSize = 0x10000; // 64K.

                await using var stream = new FileStream(
                    Path.Combine(_serverOption.PickupFolder, $"email-{Guid.NewGuid():N}.eml"),
                    FileMode.CreateNew, FileAccess.Write, FileShare.None,
                    maxBufferSize, true);

                await message.WriteToAsync(stream);
            }
            else
            {
                using var client = new SmtpClient();
                if (_serverOption.LocalDomain.IsNotEmpty())
                    client.LocalDomain = _serverOption.LocalDomain;

                await client.ConnectAsync(_serverOption.SmtpServer, _serverOption.SmtpPort);

                if (_serverOption.SmtpUsername.IsNotEmpty() && _serverOption.SmtpPassword.IsNotEmpty())
                    await client.AuthenticateAsync(_serverOption.SmtpUsername, _serverOption.SmtpPassword);

                try
                {
                    await client.SendAsync(message);
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
        {
            return AsyncHelper.RunAsSync(() => ReceiveEmailAsync(maxCount));
        }

        /// <summary> </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public async Task<List<EmailMessage>> ReceiveEmailAsync(int maxCount = 10)
        {
            using var emailClient = new Pop3Client();
            await emailClient.ConnectAsync(_serverOption.PopServer, _serverOption.PopPort, true);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            if (_serverOption.PopUsername.IsNotEmpty() && _serverOption.PopPassword.IsNotEmpty())
                await emailClient.AuthenticateAsync(_serverOption.PopUsername, _serverOption.PopPassword);

            var emails = new List<EmailMessage>();
            for (var i = 0; i < emailClient.Count && i < maxCount; i++)
            {
                var message = await emailClient.GetMessageAsync(i);
                var emailMessage = new EmailMessage
                {
                    Content = message.HtmlBody.IsNotEmpty() ? message.HtmlBody : message.TextBody,
                    Subject = message.Subject
                };
                emailMessage.Receivers.AddRange(message.To.Select(x => (MailboxAddress) x)
                    .Select(x => new MailAddress {Email = x.Address, Name = x.Name}));
                emailMessage.Senders.AddRange(message.From.Select(x => (MailboxAddress) x)
                    .Select(x => new MailAddress {Email = x.Address, Name = x.Name}));
                emails.Add(emailMessage);
            }

            return emails;
        }
    }
}