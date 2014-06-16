using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.Validators;
using KwasantCore.Managers.APIManager.Packagers.Twilio;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using Twilio;
using Microsoft.WindowsAzure;
using KwasantCore.Services;
using Utilities;
using Encoding = System.Text.Encoding;

namespace KwasantCore.Managers.CommunicationManager
{
    public class CommunicationManager
    {
        //Register for interesting events

        public void SubscribeToAlerts()
        {
            AlertManager.AlertCustomerCreated += NewCustomerWorkflow;
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(DateTime createdDate, UserDO userDO)
        {
            GenerateWelcomeEmail(userDO);  
        }

        public void GenerateWelcomeEmail(UserDO curUser)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailDO curEmail = new EmailDO();
            curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(GetFromEmail(), GetFromName());
            curEmail.AddEmailRecipient(EmailParticipantType.TO, curUser.EmailAddress);
            curEmail.Subject = "Welcome to Kwasant";
            Email _email = new Email(uow);
            _email.SendTemplate("welcome_to_kwasant_v2", curEmail, null); 
        }

        public void DispatchInvitations(EventDO curEventDO)
        {
            //if (curEventDO.EventChangeRecord != null)
           // {
           //     GenerateChangedMeetingEmails(curEventDO);
           // }
       // else
            //{
                GenerateStandardInvitation(curEventDO);
                curEventDO.Status = "Processed";
            // }
        }

        //processing involves creating a DDay event using the DDay library. This allows us to easily generate the ICS formats needed
        public iCalendar GenerateICSAttachment(IUnitOfWork uow, EventDO eventDO)
        {
            iCalendar ddayCalendar = new iCalendar();


            DDayEvent dDayEvent = new DDayEvent();

            //configure start and end time
            if (eventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(eventDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(eventDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            //configure text fields
            dDayEvent.Location = eventDO.Location;
            dDayEvent.Description = eventDO.Description;
            dDayEvent.Summary = eventDO.Summary;

            //generate ics attendee structures
            foreach (AttendeeDO attendee in eventDO.Attendees)
            {
                dDayEvent.Attendees.Add(new KwasantICS.DDay.iCal.DataTypes.Attendee()
                {
                    CommonName = attendee.Name,
                    Type = "INDIVIDUAL",
                    Role = "REQ-PARTICIPANT",
                    ParticipationStatus = ParticipationStatus.NeedsAction,
                    RSVP = true,
                    Value = new Uri("mailto:" + attendee.EmailAddress),
                });
                attendee.Event = eventDO;
            }
            //final assembly of event
            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };
            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;
            return ddayCalendar;

        }

        //this  generates the outbound email and packages up its ics attachments
        public void GenerateStandardInvitation(EventDO eventDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emailAddressRepository = uow.EmailAddressRepository;

                EmailDO outboundEmail = GenerateInviteEmail(uow, eventDO);
                eventDO.Emails.Add(outboundEmail);
                AttachCalendarToEmail(GenerateICSAttachment(uow, eventDO), outboundEmail);

            }
        }

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public string GetOriginatorName(EventDO curEventDO)
        {
            UserDO originator = curEventDO.CreatedBy;
            string firstName = originator.FirstName;
            string lastName = originator.LastName;
            if (firstName != null)
            {
                if (lastName == null)
                    return firstName;

                return firstName + " " + lastName;
            }

            EmailAddressDO curEmailAddress = originator.EmailAddress;
            if (curEmailAddress.Name != null)
                return curEmailAddress.Name;

            if (curEmailAddress.Address.IsEmailAddress())
                return curEmailAddress.Address.Split(new[] { '@' })[0];

            throw new ArgumentException("Failed to extract originator info from this Event. Something needs to be there.");
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = GetAttachment(fileToAttach);

            attachmentDO.Email = emailDO;
            emailDO.Attachments.Add(attachmentDO);
        }


        private static AttachmentDO GetAttachment(string fileToAttach)
        {
            return Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/ics", Name = "invite.ics" }
                    ) { TransferEncoding = TransferEncoding.Base64 });
        }

        private EmailDO GenerateInviteEmail(IUnitOfWork uow, EventDO eventDO)
        {
            EmailDO outboundEmail = new EmailDO();

            //configure the sender information
            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");
            var fromEmailAddr = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;
            outboundEmail.From = fromEmailAddr;


            //setup attendees
            foreach (var attendeeDO in eventDO.Attendees)
            {
                var toEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendeeDO.EmailAddress.Address);
                toEmailAddress.Name = attendeeDO.Name;
                outboundEmail.AddEmailRecipient(EmailParticipantType.TO, toEmailAddress);
            }

            //configure subject
            outboundEmail.Subject = String.Format(ConfigRepository.Get("emailSubject"), GetOriginatorName(eventDO), eventDO.Summary, eventDO.StartDate);

            //configure body
            var parsedHTMLEmail = Razor.Parse(Properties.Resources.HTMLEventInvitation, new RazorViewModel(eventDO));
            var parsedPlainEmail = Razor.Parse(Properties.Resources.PlainEventInvitation,
                new RazorViewModel(eventDO));
            outboundEmail.HTMLText = parsedHTMLEmail;
            outboundEmail.PlainText = parsedPlainEmail;

            outboundEmail.EmailStatus = EmailStatus.QUEUED;

            return outboundEmail;
        }


        public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = uow.CommunicationConfigurationRepository;
            foreach (CommunicationConfigurationDO communicationConfig in communicationConfigurationRepo.GetAll().ToList())
            {
                if (communicationConfig.Type == CommunicationType.SMS)
                {
                    SendBRSMSes(bookingRequests);
                } else if (communicationConfig.Type == CommunicationType.EMAIL)
                {
                    SendBREmails(communicationConfig.ToAddress, bookingRequests, uow);
                }
                else
                {
                    throw new Exception(String.Format("Invalid communication type '{0}'", communicationConfig.Type));
                }
            }
            uow.SaveChanges();
        }

        private void SendBRSMSes(IEnumerable<BookingRequestDO> bookingRequests)
        {
            TwilioPackager twil = new TwilioPackager();
            if (bookingRequests.Any())
            {
                string toNumber = CloudConfigurationManager.GetSetting("TwilioToNumber");
                twil.SendSMS(toNumber, "Inbound Email has been received");
            }
        }

        private void SendBREmails(String toAddress, IEnumerable<BookingRequestDO> bookingRequests, IUnitOfWork uow)
        {
            EmailRepository emailRepo = uow.EmailRepository;
            const string message = "A new booking request has been created. From '{0}'.";
            foreach (BookingRequestDO bookingRequest in bookingRequests)
            {
                EmailDO outboundEmail = new EmailDO
                {
                    Subject = "New booking request!",
                    HTMLText = String.Format(message, bookingRequest.From.Address),
                    EmailStatus = EmailStatus.QUEUED
                };

                outboundEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress("scheduling@kwasant.com", "Kwasant Scheduling Services");

                outboundEmail.AddEmailRecipient(EmailParticipantType.TO, uow.EmailAddressRepository.GetOrCreateEmailAddress(toAddress));

                emailRepo.Add(outboundEmail);
            }
        }

        //This is the default originator of outbound Kwasant emails
        public static string GetFromEmail()
        {
            string email = CloudConfigurationManager.GetSetting("fromEmail");
            if (email != null)
            {
                return email;
            }
 
            throw new ArgumentException("Missing value for 'fromEmail'");
  

        }
        public static string GetFromName()
        {
            string fromName = CloudConfigurationManager.GetSetting("fromName");
            if (fromName != null)
            {
                return fromName;
            }
            throw new ArgumentException("Missing value for 'fromName'");

        }
    }
}