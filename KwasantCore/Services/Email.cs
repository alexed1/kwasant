using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Validations;
using FluentValidation;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using StructureMap;
using Utilities;


namespace KwasantCore.Services
{
    public class Email
    {
        private EventValidator _curEventValidator;
        private readonly EmailAddress _emailAddress;

        #region Constructor


      
        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        /// 
        //this constructor enables the creation of an email that doesn't necessarily have anything to do with an Event. It gets called by the other constructors
        public Email(EmailAddress emailAddress)
        {
            _emailAddress = emailAddress;
            _curEventValidator = new EventValidator();
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        [ObsoleteAttribute("Use directly uow.EnvelopeRepository.ConfigureTemplatedEmail method.")]
        public EnvelopeDO SendTemplate(IUnitOfWork uow, string templateName, IEmail message, Dictionary<string, string> mergeFields)
        {
            return uow.EnvelopeRepository.ConfigureTemplatedEmail(message, templateName, mergeFields);
        }

        [ObsoleteAttribute("Use directly uow.EnvelopeRepository.ConfigurePlainEmail method.")]
        public EnvelopeDO Send(IUnitOfWork uow, EmailDO emailDO)
        {
            return uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
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

      
        public static EmailDO ConvertMailMessageToEmail(IEmailRepository emailRepository, MailMessage mailMessage)
        {
            return ConvertMailMessageToEmail<EmailDO>(emailRepository, mailMessage);            
        }

        public static TEmailType ConvertMailMessageToEmail<TEmailType>(IGenericRepository<TEmailType> emailRepository, MailMessage mailMessage)
            where TEmailType : EmailDO, new()
        {
            String body = String.Empty;
            String plainBody = mailMessage.Body;
            if (!mailMessage.IsBodyHtml)
            {
                foreach (var av in mailMessage.AlternateViews)
                {
                    if (av.ContentType.MediaType == "text/html")
                    {
                        body = new StreamReader(av.ContentStream).ReadToEnd();
                        break;
                    }
                }
                foreach (var av in mailMessage.AlternateViews)
                {
                    if (av.ContentType.MediaType == "text/plain")
                    {
                        plainBody = new StreamReader(av.ContentStream).ReadToEnd();
                        break;
                    }
                }
            }
            if (String.IsNullOrEmpty(body))
                body = mailMessage.Body;

            String strDateRecieved = String.Empty;
            strDateRecieved = mailMessage.Headers["Date"];

            DateTimeOffset dateRecieved;
            if (!DateTimeOffset.TryParse(strDateRecieved, out dateRecieved))
                dateRecieved = DateTimeOffset.Now;

            String strDateCreated = String.Empty;
            strDateCreated = mailMessage.Headers["Date"];

            DateTimeOffset dateCreated;
            if (!DateTimeOffset.TryParse(strDateCreated, out dateCreated))
                dateCreated = DateTimeOffset.Now;

            TEmailType emailDO = new TEmailType
            {                
                Subject = mailMessage.Subject,
                HTMLText = body,
                PlainText = plainBody,
                DateReceived = dateRecieved,
                DateCreated = dateCreated,
                Attachments = mailMessage.Attachments.Select(CreateNewAttachment).Union(mailMessage.AlternateViews.Select(CreateNewAttachment)).Where(a => a != null).ToList(),
                Events = null
            };
            var uow = emailRepository.UnitOfWork;

            emailDO.From = GenerateEmailAddress(uow, mailMessage.From);
            foreach (var addr in mailMessage.To.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.To, addr);    
            }
            foreach (var addr in mailMessage.Bcc.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.Bcc, addr);
            }
            foreach (var addr in mailMessage.CC.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.Cc, addr);
            }

            emailDO.Attachments.ForEach(a => a.Email = emailDO);
            //emailDO.EmailStatus = EmailStatus.QUEUED; we no longer want to set this here. not all Emails are outbound emails. This should only be set in functions like Event#Dispatch
            emailDO.EmailStatus = EmailState.Unstarted; //we'll use this new state so that every email has a valid status.
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

        public static AttachmentDO CreateNewAttachment(AlternateView av)
        {
            if (av.ContentType.MediaType == "text/html")
                return null;

            AttachmentDO att = new AttachmentDO
            {
                OriginalName = String.IsNullOrEmpty(av.ContentType.Name)? av.ContentType.MediaType : "File",
                Type = av.ContentType.MediaType,
            };

            att.SetData(av.ContentStream);
            return att;
        }


       
        public EmailDO GenerateBasicMessage(IUnitOfWork uow, EmailAddressDO curEmailAddress,string subject, string message, string fromAddress ,string toRecipient)
        {
            ValidateEmailAddress(curEmailAddress);
            EmailDO curEmail = new EmailDO()
            {
                Subject = subject,
                PlainText = message,
                HTMLText = message
            };
            curEmail = AddFromAddress(uow, curEmail,fromAddress);
            curEmail = AddSingleRecipient(uow, curEmail, toRecipient);
            return curEmail;
        }

        public void SendAlertEmail()
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                EmailAddressDO curEmailAddress = new EmailAddressDO("ops@kwasant.com");
                EmailDO curEmail = new EmailDO();
                string message = "Alert! Kwasant Error Reported: EmailSendFailure";
                string subject = "Alert! Kwasant Error Reported: EmailSendFailure";
                curEmail = GenerateBasicMessage(uow, curEmailAddress, subject, message, fromAddress, "ops@kwasant.com");
                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }
        }



        public void ValidateEmailAddress(EmailAddressDO curEmailAddress)
        {
            EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
            emailAddressValidator.ValidateAndThrow(curEmailAddress);

        }

        public EmailDO AddSingleRecipient(IUnitOfWork uow, EmailDO curEmail, string toRecipient)
        {
            curEmail.Recipients = new List<RecipientDO>()
                                         {
                                              new RecipientDO()
                                                 {
                                                   EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(toRecipient),
                                                   EmailParticipantType = EmailParticipantType.To
                                                 }
                                         };
            return curEmail;
        }


        public EmailDO AddFromAddress(IUnitOfWork uow, EmailDO curEmail, string fromAddress)
        {
            curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
            return curEmail;
        }

    }
}
