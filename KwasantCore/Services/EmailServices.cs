using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;

namespace KwasantCore.Services
{
    public class EmailServices
    {
        private readonly IUnitOfWork _uow;
        private readonly EmailDO _emailDO;

        #region Members

        private readonly MandrillPackager _mandrillApi;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        public EmailServices(IUnitOfWork uow, EmailDO emailDO)
        {
            _uow = uow;
            _emailDO = emailDO;
            _mandrillApi = new MandrillPackager();
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public void SendTemplate(string templateName, EmailDO message, Dictionary<string, string> mergeFields)
        {
            _mandrillApi.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        public void Send()
        {
            _mandrillApi.PostMessageSend(_emailDO);
            _emailDO.StatusID = EmailStatusConstants.SENT;
            _uow.SaveChanges();
        }


        public void Ping()
        {
            string results = _mandrillApi.PostPing();
            Debug.WriteLine(results);
        }

        #endregion

      
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
                Invitations = null
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
    }
}
