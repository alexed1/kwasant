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
        private  EmailDO _curEmailDO;
        private EventValidator _curEventValidator;
        private IEmailRepository _curEmailRepository;
        private IEmailEmailAddressRepository _emailEmailAddressRepository;
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
            _curEmailRepository = new EmailRepository(_uow);
            _emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);
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
            MandrillPackager.PostMessageSend(_curEmailDO);
            _curEmailDO.Status = EmailStatus.DISPATCHED;
            _curEmailRepository.Add(_curEmailDO);
            _curEmailRepository.UnitOfWork.SaveChanges();
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

      
        public static EmailDO ConvertMailMessageToEmail(IEmailRepository emailRepository, IEmailEmailAddressRepository emailAddressRepository, MailMessage mailAddress)
        {
            return ConvertMailMessageToEmail<EmailDO>(emailRepository, emailAddressRepository, mailAddress);
        }

        public static TEmailType ConvertMailMessageToEmail<TEmailType>(IGenericRepository<TEmailType> emailRepository, IEmailEmailAddressRepository emailAddressRepository, MailMessage mailAddress)
            where TEmailType : EmailDO, new()
        {
            TEmailType emailDO = new TEmailType
            {
                Subject = mailAddress.Subject,
                Text = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                Events = null
            };

            emailDO.AddEmailParticipant(EmailParticipantType.FROM, emailAddressRepository, GetEmailAddress(mailAddress.From));
            foreach (var addr in mailAddress.To.Select(GetEmailAddress))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.TO, emailAddressRepository, addr);    
            }
            foreach (var addr in mailAddress.Bcc.Select(GetEmailAddress))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.BCC, emailAddressRepository, addr);
            }
            foreach (var addr in mailAddress.CC.Select(GetEmailAddress))
            {
                emailDO.AddEmailParticipant(EmailParticipantType.CC, emailAddressRepository, addr);
            }


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

       


        public EmailDO CreateStandardInviteEmail(EventDO curEventDO)
        {
            _curEventValidator.ValidateEvent(curEventDO);
            string fromEmail = CommunicationManager.GetFromEmail();
            string fromName = CommunicationManager.GetFromName(); 

            EmailDO createdEmail = new EmailDO();
            var emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);
            createdEmail.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO { Address = fromEmail, Name = fromName });

            foreach (var attendee in curEventDO.Attendees)
            {
                createdEmail.AddEmailParticipant(EmailParticipantType.TO, emailEmailAddressRepository, new EmailAddressDO { Address = attendee.EmailAddress, Name = attendee.Name });
            }
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
                
                createdEmail.AddEmailParticipant(EmailParticipantType.BCC, emailEmailAddressRepository, archiveAddress);
            }

            _curEmailRepository.Add(createdEmail);
            _curEmailRepository.UnitOfWork.SaveChanges();
            return createdEmail;
        }

        //FIX THIS: currently generates an EF exception.
        public void Dispatch(EmailDO curEmail)
        {
            curEmail.Status = EmailStatus.QUEUED;
            _curEmailRepository.Add(curEmail);
            _uow.SaveChanges();
        }

    }
}
