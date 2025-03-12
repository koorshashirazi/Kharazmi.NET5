﻿#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kharazmi.Extensions;
using MimeKit;

#endregion

namespace Kharazmi.Mvc.MailServer
{
    public class EmailMessageBuilder : IEmailMessageBuilder
    {
        private readonly MimeMessage _message;
        private readonly BodyBuilder _bodyBuilder;

        private EmailMessageBuilder()
        {
            _message = new MimeMessage();
            _bodyBuilder = new BodyBuilder();
        }

        public static IEmailMessageBuilder Create()
        {
            return new EmailMessageBuilder();
        }


        IEmailMessageBuilder IEmailMessageBuilder.WithHeader(MailHeaders headers, string fromAddress)
        {
            if (headers is null) return this;

            var host = fromAddress.Split('@').Last();

            if (headers.MessageId.IsNotEmpty()) _message.MessageId = $"<{headers.MessageId}@{host}>";

            if (headers.InReplyTo.IsNotEmpty()) _message.InReplyTo = $"<{headers.InReplyTo}@{host}>";

            if (headers.References.IsNotEmpty()) _message.References.Add($"<{headers.References}@{host}>");

            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithAttachmentFiles(List<string> pathFiles)
        {
            if (pathFiles is null || pathFiles.Count <= 0)
                return this;
            foreach (var attachmentFile in pathFiles.Where(attachmentFile =>
                attachmentFile.IsNotEmpty() && File.Exists(attachmentFile)))
                _bodyBuilder.Attachments.Add(attachmentFile);

            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithReplyTos(IReadOnlyCollection<MailAddress> mailAddress)
        {
            if (mailAddress is null || mailAddress.Count <= 0) return this;
            foreach (var rt in mailAddress) _message.ReplyTo.Add(new MailboxAddress(rt.Name ?? string.Empty, rt.Email));

            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithBcc(IReadOnlyCollection<MailAddress> mailAddress)
        {
            if (mailAddress is null || mailAddress.Count <= 0) return this;
            foreach (var bcc in mailAddress) _message.Bcc.Add(new MailboxAddress(bcc.Name ?? string.Empty, bcc.Email));

            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithCc(IReadOnlyCollection<MailAddress> mailAddress)
        {
            if (mailAddress is null || mailAddress.Count <= 0) return this;

            foreach (var cc in mailAddress) _message.Cc.Add(new MailboxAddress(cc.Name ?? string.Empty, cc.Email));

            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithSender(MailAddress mailAddress)
        {
            _message.From.Add(new MailboxAddress(mailAddress.Name, mailAddress.Email));
            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithReceiver(MailAddress mailAddress)
        {
            _message.To.Add(new MailboxAddress(mailAddress.Name, mailAddress.Email));
            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithSubject(string subject)
        {
            _message.Subject = subject;
            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithSubject(string template, params object[] @params)
        {
            return ((IEmailMessageBuilder) this).WithSubject(string.Format(template, @params));
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithHtmlBody(string body)
        {
            _bodyBuilder.HtmlBody = body;
            return this;
        }

        IEmailMessageBuilder IEmailMessageBuilder.WithStringBody(string template, params object[] @params)
        {
            _bodyBuilder.TextBody = string.Format(template, @params);
            return this;
        }

        MimeMessage IEmailMessageBuilder.Build()
        {
            _message.Body = _bodyBuilder.ToMessageBody();
            return _message;
        }
    }
}