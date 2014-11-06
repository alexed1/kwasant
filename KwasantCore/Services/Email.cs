using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Validations;
using FluentValidation;
using StructureMap;
using Utilities;
using Utilities.Logging;


namespace KwasantCore.Services
{
    public class Email
    {
        private EventValidator _curEventValidator;
        private readonly EmailAddress _emailAddress;

        #region Constructor
        public Email()
        {
        }

      
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

        public void SendUserSettingsNotification(IUnitOfWork uow, UserDO submittedUserData) 
        {
            EmailDO curEmail = new EmailDO();
            curEmail.From = submittedUserData.EmailAddress;
            curEmail.AddEmailRecipient(EmailParticipantType.To, submittedUserData.EmailAddress);
            curEmail.Subject = "User Settings Notification";
            //new Email(uow).SendTemplate(uow, "User_Settings_Notification", curEmail, null);
            uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "User_Settings_Notification", null);
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
                    av.ContentStream.Position = 0;
                    if (av.ContentType.MediaType == "text/html")
                    {
                        body = new StreamReader(av.ContentStream).ReadToEnd();
                        break;
                    }

                    if (av.ContentType.MediaType == "text/plain")
                    {
                        plainBody = new StreamReader(av.ContentStream).ReadToEnd();
                    }
                }
            }
            if (String.IsNullOrEmpty(body))
                body = mailMessage.Body;

            String strDateReceived = String.Empty;
            strDateReceived = mailMessage.Headers["Date"];

            DateTimeOffset dateReceived;
            if (!DateTimeOffset.TryParse(strDateReceived, out dateReceived))
                dateReceived = DateTimeOffset.Now;

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
                DateReceived = dateReceived,
                DateCreated = dateCreated,
                Attachments = mailMessage.Attachments.Select(CreateNewAttachment).Union(mailMessage.AlternateViews.Select(CreateNewAttachment)).Where(a => a != null).ToList(),
                Events = null
            };
            var uow = emailRepository.UnitOfWork;

            var fromAddress = GenerateEmailAddress(uow, mailMessage.From);
            emailDO.From = fromAddress;
            
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
                OriginalName = String.IsNullOrEmpty(av.ContentType.Name) ? "unnamed" : av.ContentType.Name,
                Type = av.ContentType.MediaType,
                ContentID = av.ContentId
            };

            att.SetData(av.ContentStream);
            return att;
        }
        
        public EmailDO GenerateBasicMessage(IUnitOfWork uow, string subject, string message, string fromAddress ,string toRecipient)
        {
            new EmailAddressValidator().Validate(new EmailAddressDO(toRecipient));
            EmailDO curEmail = new EmailDO
            {
                Subject = subject,
                PlainText = message,
                HTMLText = message
            };
            curEmail = AddFromAddress(uow, curEmail,fromAddress);
            curEmail = AddSingleRecipient(uow, curEmail, toRecipient);
            return curEmail;
        }

        public void SendAlertEmail(string subject, string message = null)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                EmailDO curEmail = new EmailDO();
                curEmail = GenerateBasicMessage(uow, subject, message ?? subject, fromAddress, "ops@kwasant.com");
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
            var from = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
            curEmail.From = from;
            curEmail.FromID = from.Id;
            return curEmail;
        }

        public static void ProcessReceivedMessage(IUnitOfWork uow, EmailDO curEmail, MailMessage message)
        {
            BookingRequestDO existingBookingRequest = Conversation.Match(uow, curEmail);

            if (existingBookingRequest != null)
            {
                Conversation.AddEmail(uow, existingBookingRequest, curEmail);
            }
            else
            {
                uow.EmailRepository.Remove(curEmail);
                BookingRequestDO bookingRequest = ConvertMailMessageToEmail(uow.BookingRequestRepository, message);
                (new BookingRequest()).Process(uow, bookingRequest);
                uow.SaveChanges();

                //Fix the HTML text
                var attachmentSubstitutions =
                    bookingRequest.Attachments.Where(a => !String.IsNullOrEmpty(a.ContentID))
                        .ToDictionary(a => a.ContentID, a => a.Id);

                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                //The following fixes inline images
                if (attachmentSubstitutions.Any())
                {
                    var curBody = bookingRequest.HTMLText;
                    foreach (var keyToReplace in attachmentSubstitutions.Keys)
                    {
                        var keyStr = String.Format("cid:{0}", keyToReplace);
                        curBody = curBody.Replace(keyStr,
                            String.Format(fileViewURLStr, attachmentSubstitutions[keyToReplace]));
                    }
                    bookingRequest.HTMLText = curBody;
                    uow.SaveChanges();
                }

                AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
            }
        }

        public void SendLoginCredentials(IUnitOfWork uow, string toRecipient, string newPassword) 
        {
            string credentials = "<br/> Email : " + toRecipient + "<br/> Password : " + newPassword;
            string fromAddress = ObjectFactory.GetInstance<IConfigRepository>().Get("EmailFromAddress_DirectMode");
            EmailDO emailDO = GenerateBasicMessage(uow, "Kwasant Credentials", null, fromAddress, toRecipient);
            uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, "e4da63fd-2459-4caf-8e4f-b4d6f457e95a",
                    new Dictionary<string, string>
                    {
                        {"credentials_string", credentials}
                    });
            uow.SaveChanges();
        }
    }
}
