using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using Data.Tools.Managers;
using StructureMap;

namespace Data.Tools
{
    public static class EmailHelper
    {
        public static EmailDO ConvertMailMessageToEmail(IEmailRepository emailRepository, MailMessage mailAddress)
        {
            return ConvertMailMessageToEmail<EmailDO>(emailRepository, mailAddress);
        }

        public static TEmailType ConvertMailMessageToEmail<TEmailType>(IGenericRepository<TEmailType> emailRepository, MailMessage mailAddress)
            where TEmailType : EmailDO, new()
        {
            TEmailType emailDO = new TEmailType
            {
                From = GetEmailAddress(mailAddress.From),
                BCC = mailAddress.Bcc.Select(GetEmailAddress).ToList(),
                CC = mailAddress.CC.Select(GetEmailAddress).ToList(),
                Subject = mailAddress.Subject,
                Text = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                To = mailAddress.To.Select(GetEmailAddress).ToList(),
                Events = null
            };
            emailDO.To.ForEach(a => a.ToEmail = emailDO);
            emailDO.CC.ForEach(a => a.BCCEmail = emailDO);
            emailDO.BCC.ForEach(a => a.CCEmail = emailDO);
            emailDO.StatusID = EmailStatusConstants.QUEUED;

            emailRepository.Add(emailDO);
            return emailDO;
        }

        private static EmailAddressDO GetEmailAddress(MailAddress address)
        {
            return new EmailAddressDO { Address = address.Address, Name = address.DisplayName };
        }

        public static AttachmentDO CreateNewAttachment(Attachment attachment)
        {
            AttachmentDO att = new AttachmentDO
            {
                OriginalName = attachment.Name,
                Type = attachment.ContentType.MediaType,
            };
            att.SetData(attachment.ContentStream);
            return att;
        }

        public static void SendEmail(EmailDO emailDO)
        {
            new EmailManager().Send(emailDO);
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();

            emailDO.StatusID = EmailStatusConstants.SENT;
            uow.SaveChanges();
        }
    }
}
