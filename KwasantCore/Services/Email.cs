using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using Data.Validators;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.Managers.CommunicationManager;
using Microsoft.WindowsAzure;
using StructureMap;

namespace KwasantCore.Services
{
    public class Email
    {
        private  IUnitOfWork _uow;
        private  EmailDO _emailDO;
        private EventValidator _curEventValidator;

        #region Members

        private readonly MandrillPackager _mandrillApi;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        /// 
           
        public Email(IUnitOfWork uow)
        {
            _uow = uow;
            _mandrillApi = ObjectFactory.GetInstance<MandrillPackager>();
            _curEventValidator = new EventValidator();
        }

        public Email(IUnitOfWork uow, EmailDO emailDO) : this(uow) //this can probably be simplified to a single constructor. Do we really want to pass emailDO in?
        {
            
            _emailDO = emailDO;
            
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
        public void Send(EmailDO curEmailDO)
        {
            _emailDO = curEmailDO;
            Send();
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

       


        public EmailDO CreateStandardInviteEmail(EventDO curEventDO)
        {
            _curEventValidator.ValidateEvent(curEventDO);
            string fromEmail = CommunicationManager.GetFromEmail();
            string fromName = CommunicationManager.GetFromName(); 

            EmailDO createdEmail = new EmailDO();
            createdEmail.From = new EmailAddressDO { Address = fromEmail, Name = fromName };
            createdEmail.To = curEventDO.Attendees.Select(a => new EmailAddressDO { Address = a.EmailAddress, Name = a.Name }).ToList();
            createdEmail.Subject = "Invitation via Kwasant: " + curEventDO.Summary + "@ " + curEventDO.StartDate;
            createdEmail.Text = "This is a Kwasant Event Request. For more information, see http://www.kwasant.com";
            createdEmail.Status = EmailStatus.QUEUED;

            if (CloudConfigurationManager.GetSetting("ArchiveOutboundEmail") == "true")
            {
                EmailAddressDO archiveAddress = new EmailAddressDO();
                archiveAddress.Address = CloudConfigurationManager.GetSetting("ArchiveEmailAddress");
                archiveAddress.Name = archiveAddress.Address;
                EmailAddressValidator curEmailAddressValidator = new EmailAddressValidator();
                curEmailAddressValidator.ValidateAndThrow(archiveAddress);
                
                createdEmail.BCC.Add(archiveAddress);
            }

            return createdEmail;
        }
    }
}
