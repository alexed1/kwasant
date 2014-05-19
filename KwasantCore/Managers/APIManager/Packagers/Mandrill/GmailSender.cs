﻿using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.Entities;

namespace KwasantCore.Managers.APIManager.Packagers.Mandrill
{
    public static class GmailSender
    {
        public static void Send(EmailDO email)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential() { UserName = "kwasantintake@gmail.com", Password = "wymcivlctelmbazc" }
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
            var htmlView = AlternateView.CreateAlternateViewFromString(email.Text, Encoding.UTF8, "text/html");
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
