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
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.Managers.CommunicationManager;
using Microsoft.WindowsAzure;

namespace KwasantCore.Services
{
    public class Email
    {
        private IUnitOfWork _uow;
        private EmailDO _curEmailDO;
        private EventValidator _curEventValidator;
        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        /// 

        //this constructor enables the creation of an email that doesn't necessarily have anything to do with an Event. It gets called by the other constructors
        public Email(IUnitOfWork uow)
        {
            _uow = uow;
            _curEventValidator = new EventValidator();
        }
        public Email(IUnitOfWork uow, EventDO eventDO): this(uow)
        {
            _curEmailDO = CreateStandardInviteEmail(eventDO);
        }

        public Email(IUnitOfWork uow, EmailDO curEmailDO) : this(uow)
        {
            //should add validation here
            _curEmailDO = curEmailDO;
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
            var gmailPackager = new GmailPackager(_curEmailDO);
            gmailPackager.Send();
            _curEmailDO.Status = EmailStatus.DISPATCHED;
            _uow.EmailRepository.Add(_curEmailDO);
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
                Subject = mailAddress.Subject,
                HTMLText = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                Events = null
            };
            var uow = emailRepository.Database.UnitOfWork;

            emailDO.AddEmailParticipant(EmailParticipantType.FROM, GenerateEmailAddress(uow, mailAddress.From));
            foreach (var addr in mailAddress.To.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.TO, addr);    
            }
            foreach (var addr in mailAddress.Bcc.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.BCC, addr);
            }
            foreach (var addr in mailAddress.CC.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.CC, addr);
            }

            emailDO.Attachments.ForEach(a => a.Email = emailDO);
            emailDO.Status = EmailStatus.QUEUED;

            emailRepository.Add(emailDO);
            return emailDO;
        }

        public static EmailAddressDO GenerateEmailAddress(IUnitOfWork uow, MailAddress address)
        {
            return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
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
            createdEmail.AddEmailParticipant(EmailParticipantType.FROM, _uow.EmailAddressRepository.GetOrCreateEmailAddress(fromEmail, fromName));

            foreach (var attendee in curEventDO.Attendees)
            {
                createdEmail.AddEmailParticipant(EmailParticipantType.TO, _uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee.EmailAddress, attendee.Name));
            }
            createdEmail.Subject = "Invitation via Kwasant: " + curEventDO.Summary + "@ " + curEventDO.StartDate;
            createdEmail.HTMLText = "This is a Kwasant Event Request. For more information, see http://www.kwasant.com";
            createdEmail.Status = EmailStatus.QUEUED;

            if (CloudConfigurationManager.GetSetting("ArchiveOutboundEmail") == "true")
            {
                string archiveEmailAddress = CloudConfigurationManager.GetSetting("ArchiveEmailAddress");
                EmailAddressDO archiveAddress = _uow.EmailAddressRepository.GetOrCreateEmailAddress(archiveEmailAddress, archiveEmailAddress);
                
                EmailAddressValidator curEmailAddressValidator = new EmailAddressValidator();
                curEmailAddressValidator.ValidateAndThrow(archiveAddress);
                
                createdEmail.AddEmailParticipant(EmailParticipantType.BCC, archiveAddress);
            }

            _uow.EmailRepository.Add(createdEmail);
            _uow.SaveChanges();
            return createdEmail;
        }
    }
}
