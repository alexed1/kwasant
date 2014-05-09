using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using StructureMap;

namespace KwasantCore.Services
{
    public class Email
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
        public Email(IUnitOfWork uow, EmailDO emailDO)
        {
            _uow = uow;
            _emailDO = emailDO;
            _mandrillApi = ObjectFactory.GetInstance<MandrillPackager>();
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
            _emailDO.Status = EmailStatus.SENT;
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
                From = GenerateEmailAddress(mailAddress.From),
                BCC = mailAddress.Bcc.Select(GenerateEmailAddress).ToList(),
                CC = mailAddress.CC.Select(GenerateEmailAddress).ToList(),
                Subject = mailAddress.Subject,
                Text = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                To = mailAddress.To.Select(GenerateEmailAddress).ToList(),
                Events = null
            };
            emailDO.To.ForEach(a => a.ToEmail = emailDO);
            emailDO.CC.ForEach(a => a.BCCEmail = emailDO);
            emailDO.BCC.ForEach(a => a.CCEmail = emailDO);
            emailDO.Attachments.ForEach(a => a.Email = emailDO);
            emailDO.Status = EmailStatus.QUEUED;

            emailRepository.Add(emailDO);
            return emailDO;
        }

        public static EmailAddressDO GenerateEmailAddress(MailAddress address)
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
