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
                Credentials = new NetworkCredential() {UserName = "rjrudman@gmail.com", Password = "zvavlfkbefvzresj"}
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
            //mailMessage.Body = email.Text;
            mailMessage.IsBodyHtml = true;
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.Text, null, "text/html"));


            foreach (var attachment in email.Attachments)
            {

                var vCT = new ContentType("text/calendar");
                vCT.CharSet = "UTF-8";
                vCT.Parameters.Add("method", "REQUEST");

                var av = new AlternateView(attachment.GetData(), vCT);
                av.TransferEncoding = TransferEncoding.SevenBit;

                mailMessage.AlternateViews.Add(av);
 
                var ct = new ContentType(attachment.Type);
                ct.MediaType = attachment.Type;
                ct.Name = attachment.OriginalName;
                
                //var lr = new LinkedResource(attachment.GetData(),ct);
                //lr.TransferEncoding = TransferEncoding.Base64;

                //av.LinkedResources.Add(lr);

                //if (attachment.OriginalName.EndsWith(".ics"))
                //{
                   
                //}
            }

            smtpClient.Send(mailMessage);
            
        }
    }
}
