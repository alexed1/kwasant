using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.Entities;
using Utilities;

namespace KwasantCore.Managers.APIManager.Packagers
{
    public class GmailPackager : IEmailPackager
    {
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
            if (handler != null) handler(errorCode, name, message, emailID); ;
        }

        //Note that at the moment, we actually are submitting through SendGrid, not Gmail.
        public void Send(EnvelopeDO envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException("envelope");
            if (!string.Equals(envelope.Handler, EnvelopeDO.GmailHander))
                throw new ArgumentException("This envelope should not be handled with Gmail.", "envelope");
            
            var email = envelope.Email;
            if (email == null)
                throw new ArgumentException("Envelope email is null", "envelope");

            try
            {
                var smtpClient = new SmtpClient(ConfigRepository.Get("OutboundEmailHost"),
                                                ConfigRepository.Get<int>("OutboundEmailPort"))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials =
                        new NetworkCredential()
                        {
                            UserName = ConfigRepository.Get("OutboundUserName"),
                            Password = ConfigRepository.Get("OutboundUserPassword")
                        }
                };

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(email.From.Address, email.From.Name);

                foreach (var toEmail in email.To)
                {
                    mailMessage.To.Add(new MailAddress(toEmail.Address, toEmail.Name));
                }

                foreach (var bcc in email.BCC)
                {
                    mailMessage.Bcc.Add(new MailAddress(bcc.Address, bcc.Name));
                }

                foreach (var cc in email.CC)
                {
                    mailMessage.CC.Add(new MailAddress(cc.Address, cc.Name));
                }

                mailMessage.Subject = email.Subject;
                mailMessage.IsBodyHtml = true;

                if (email.PlainText == null || email.HTMLText == null)
                    throw new ArgumentException("Trying to send an email that doesn't have both an HTML and plain text body");

                var htmlView = AlternateView.CreateAlternateViewFromString(email.HTMLText, Encoding.UTF8, "text/html");
                var plainView = AlternateView.CreateAlternateViewFromString(email.PlainText, Encoding.UTF8, "text/plain");

                if (!ConfigRepository.Get<bool>("compressEmail"))
                {
                    htmlView.TransferEncoding = TransferEncoding.SevenBit;
                    plainView.TransferEncoding = TransferEncoding.SevenBit;
                }

                mailMessage.AlternateViews.Add(plainView);
                mailMessage.AlternateViews.Add(htmlView);

                foreach (var attachment in email.Attachments)
                {
                    if (attachment.OriginalName.EndsWith(".ics"))
                    {
                        var vCT = new ContentType("text/calendar");
                        vCT.CharSet = "UTF-8";
                        vCT.Parameters.Add("method", "REQUEST");

                        var av = new AlternateView(attachment.GetData(), vCT);
                        av.TransferEncoding = TransferEncoding.SevenBit;
                        mailMessage.AlternateViews.Add(av);
                    }

                    var ct = new ContentType(attachment.Type);
                    ct.MediaType = attachment.Type;
                    ct.Name = attachment.OriginalName;

                    var att = new LinkedResource(attachment.GetData(), ct);
                    if (!String.IsNullOrEmpty(attachment.ContentID))
                        att.ContentId = attachment.ContentID;

                    att.TransferEncoding = TransferEncoding.Base64;

                    htmlView.LinkedResources.Add(att);
                }

                try
                {
                    smtpClient.Send(mailMessage);
                    OnEmailSent(email.Id);
                }
                catch (SmtpException ex)
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
