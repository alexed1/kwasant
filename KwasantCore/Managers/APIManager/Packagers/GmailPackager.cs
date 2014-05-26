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
        public void Send(EmailDO email)
        {
            var smtpClient = new SmtpClient(ConfigRepository.Get("OutboundEmailHost"), ConfigRepository.Get<int>("OutboundEmailPort"))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential() { UserName = ConfigRepository.Get("OutboundUserName"), Password = ConfigRepository.Get("OutboundUserPassword") }
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

            smtpClient.Send(mailMessage);
            
        }
    }
}
