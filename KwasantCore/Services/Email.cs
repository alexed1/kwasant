using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using Data.Validators;
using FluentValidation;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.Managers.CommunicationManager;
using Microsoft.WindowsAzure;

namespace KwasantCore.Services
{
    public class Email
    {
        private  IUnitOfWork _uow;
        public  EmailDO EmailDO;
        private EventValidator _curEventValidator;

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        /// 
           
        public Email(IUnitOfWork uow, EventDO eventDO)
        {
            _uow = uow;
            _curEventValidator = new EventValidator();
            EmailDO = CreateStandardInviteEmail(eventDO);
        }

        public Email(IUnitOfWork uow, EmailDO emailDO)
        {
            _uow = uow;
            _curEventValidator = new EventValidator();
            EmailDO = emailDO;
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public void SendTemplate(string templateName, EmailDO message, Dictionary<string, string> mergeFields)
        {
            MandrillPackager.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        public void Send()
        {
            MandrillPackager.PostMessageSend(EmailDO);
            EmailDO.Status = EmailStatus.DISPATCHED;
            _uow.SaveChanges();
        }

        public static void InitialiseWebhook(String url)
        {
            MandrillPackager.InitialiseWebhook(url);
        }

        public static void HandleWebhookResponse(String responseStr)
        {
            MandrillPackager.HandleWebhookResponse(responseStr);
        }

        public static void Ping()
        {
            string results = MandrillPackager.PostPing();
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
