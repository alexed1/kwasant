using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.Entities;
using KwasantCore.ExternalServices;
using SendGrid;
using StructureMap;
using Utilities;

namespace KwasantCore.Managers.APIManagers.Packagers
{
    public class SendGridPackager : IEmailPackager
    {
        private readonly IConfigRepository _configRepository;

        public SendGridPackager(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
        }

        public delegate void EmailSuccessArgs(int emailID);
        public static event EmailSuccessArgs EmailSent;

        private static void OnEmailSent(int emailid)
        {
            EmailSuccessArgs handler = EmailSent;
            if (handler != null) handler(emailid);
        }

        public delegate void EmailRejectedArgs(string rejectReason, int emailID);
        public static event EmailRejectedArgs EmailRejected;

        private static void OnEmailRejected(string rejectReason, int emailid)
        {
            EmailRejectedArgs handler = EmailRejected;
            if (handler != null) handler(rejectReason, emailid);
        }

        public delegate void EmailCriticalErrorArgs(int errorCode, string name, string message, int emailID);
        public static event EmailCriticalErrorArgs EmailCriticalError;

        private static void OnEmailCriticalError(int errorCode, string name, string message, int emailID)
        {
            EmailCriticalErrorArgs handler = EmailCriticalError;
            if (handler != null) handler(errorCode, name, message, emailID);
        }

        //Note that at the moment, we actually are submitting through SendGrid, not Gmail.
        public async void Send(EnvelopeDO envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException("envelope");
            if (!string.Equals(envelope.Handler, EnvelopeDO.SendGridHander, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(@"This envelope should not be handled with Gmail.", "envelope");
            if (envelope.Email == null)
                throw new ArgumentException(@"This envelope has no Email.", "envelope");
            if (envelope.Email.Recipients.Count == 0)
                throw new ArgumentException(@"This envelope has no recipients.", "envelope");
            
            var email = envelope.Email;
            if (email == null)
                throw new ArgumentException(@"Envelope email is null", "envelope");

            try
            {
                var mailMessage = new SendGridMessage() { From = new MailAddress(email.From.Address, email.From.Name) };

                if (email.ReplyTo != null)
                {
                    mailMessage.ReplyTo = new[] { new MailAddress(email.ReplyTo.Address, email.ReplyTo.Name) };
                }

                mailMessage.To = email.To.Select(toEmail => new MailAddress(toEmail.Address, toEmail.Name)).ToArray();
                mailMessage.Bcc = email.BCC.Select( bcc => new MailAddress(bcc.Address, bcc.Name)).ToArray();
                mailMessage.Cc = email.CC.Select(cc => new MailAddress(cc.Address, cc.Name)).ToArray();

                mailMessage.Subject = email.Subject;

                if (email.PlainText == null || email.HTMLText == null)
                    throw new ArgumentException("Trying to send an email that doesn't have both an HTML and plain text body");

                mailMessage.Html = email.HTMLText;
                mailMessage.Text = email.PlainText;

                foreach (var attachment in email.Attachments)
                {
                    mailMessage.AddAttachment(attachment.GetData(), attachment.OriginalName);
                }

                if (!string.IsNullOrEmpty(envelope.TemplateName))
                {
                    mailMessage.EnableTemplateEngine(envelope.TemplateName);
                    if (envelope.MergeData != null)
                    {
                        foreach (var pair in envelope.MergeData)
                        {
                            mailMessage.AddSubstitution(pair.Key, new List<string>() { pair.Value });
                        }
                    }
                }

                try
                {
                    var credentials = new NetworkCredential
                    {
                        UserName = _configRepository.Get("OutboundUserName"),
                        Password = _configRepository.Get("OutboundUserPassword")
                    };
                    var transportWeb = new Web(credentials);

                    await transportWeb.DeliverAsync(mailMessage);

                    OnEmailSent(email.Id);
                }
                catch (Exception ex)
                {
                    OnEmailRejected(ex.Message, email.Id);
                }
            }
            catch (Exception ex)
            {
                OnEmailCriticalError(-1, "Unhandled exception.", ex.Message, email.Id);
                throw;
            }
        }
    }
}
