using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Validations;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using Microsoft.WindowsAzure;
using KwasantCore.Services;
using Utilities;
using Encoding = System.Text.Encoding;

namespace KwasantCore.Managers
{
    public class CommunicationManager
    {
        //Register for interesting events
        public void SubscribeToAlerts()
        {
            AlertManager.AlertCustomerCreated += NewCustomerWorkflow;
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(UserDO curUser)
        {
            GenerateWelcomeEmail(curUser);  
        }

        public void GenerateWelcomeEmail(UserDO user)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // WARNING: 'user' parameter must not be used as reference in scope of this UnitOfWork as it is attached to another UnitOfWork
                var curUser = uow.UserRepository.GetByKey(user.Id);
                EmailDO curEmail = new EmailDO();
                curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(GetFromEmail(), GetFromName());
                curEmail.AddEmailRecipient(EmailParticipantType.To, curUser.EmailAddress);
                curEmail.Subject = "Welcome to Kwasant";
                Email _email = new Email(uow);
                _email.SendTemplate("welcome_to_kwasant_v2", curEmail, null);
                uow.SaveChanges();
            }
        }

/*
        public void DispatchNegotiationRequests(IUnitOfWork uow, int negotiationID)
        {
            DispatchNegotiationRequests(uow, uow.NegotiationsRepository.GetByKey(negotiationID));
        }

        public void DispatchNegotiationRequests(IUnitOfWork uow, NegotiationDO negotiationDO)
        {
            if (negotiationDO.Attendees == null)
                return;

            foreach (var attendee in negotiationDO.Attendees)
            {
                var emailDO = new EmailDO();
                emailDO.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(GetFromEmail(), GetFromName());
                emailDO.AddEmailRecipient(EmailParticipantType.To, attendee.EmailAddress);
                emailDO.Subject = "Welcome to Kwasant";
                var htmlText = String.Format("Please click <a href='{0}NegotiationResponse/View?negotiationID={1}'>here</a> to answer some questions about your upcoming event.", Server.ServerUrl, negotiationDO.Id);

                emailDO.HTMLText = htmlText;
                emailDO.PlainText = "Please click here: " + String.Format("{0}NegotiationResponse/View?negotiationID={1}", Server.ServerUrl, negotiationDO.Id);
                emailDO.EmailStatus = EmailState.Queued;

                uow.EnvelopeRepository.CreateGmailEnvelope(emailDO);
                uow.EmailRepository.Add(emailDO);
            }
        }

        public void DispatchInvitations(IUnitOfWork uow, EventDO eventDO)
        {
            //This line is so that the Server object is compiled. Without this, Razor fails; since it's executed at runtime and the object has been optimized out when running tests.
            //var createdDate = eventDO.BookingRequest.DateCreated;
            //eventDO.StartDate = eventDO.StartDate.ToOffset(createdDate.Offset);
            //eventDO.EndDate = eventDO.EndDate.ToOffset(createdDate.Offset);

            var t = Utilities.Server.ServerUrl;
            switch (eventDO.EventStatus)
            {
                case EventState.Booking:
                    {
                        eventDO.EventStatus = EventState.DispatchCompleted;

                        var calendar = Event.GenerateICSCalendarStructure(eventDO);
                        foreach (var attendeeDO in eventDO.Attendees)
                        {
                            var emailDO = CreateInvitationEmail(uow, eventDO, attendeeDO, false);
                            var email = new Email(uow, emailDO);
                            AttachCalendarToEmail(calendar, emailDO);
                            email.Send();
                        }

                        break;
                    }
                case EventState.ReadyForDispatch:
                case EventState.DispatchCompleted:
                    //Dispatched means this event was previously created. This is a standard event change. We need to figure out what kind of update message to send
                    if (EventHasChanged(uow, eventDO))
                    {
                        eventDO.EventStatus = EventState.DispatchCompleted;
                        var calendar = Event.GenerateICSCalendarStructure(eventDO);

                        var newAttendees = eventDO.Attendees.Where(a => a.Id == 0).ToList();

                        foreach (var attendeeDO in eventDO.Attendees)
                        {
                            //Id > 0 means it's an existing attendee, so we need to send the 'update' email to them.
                            var emailDO = CreateInvitationEmail(uow, eventDO, attendeeDO, !newAttendees.Contains(attendeeDO));
                            var email = new Email(uow, emailDO);
                            AttachCalendarToEmail(calendar, emailDO);
                            email.Send();
                        }
                    }
                    else
                    {
                        //If the event hasn't changed - we don't need a new email..?
                    }
                    break;

                case EventState.ProposedTimeSlot:
                    //Do nothing
                    break;
                default:
                    throw new Exception("Invalid event status");
            }
        }
*/

/*
        private EmailDO CreateInvitationEmail(IUnitOfWork uow, EventDO eventDO, AttendeeDO attendeeDO, bool isUpdate)
        {
            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");

            var emailAddressRepository = uow.EmailAddressRepository;
            if (eventDO.Attendees == null)
                eventDO.Attendees = new List<AttendeeDO>();

            EmailDO outboundEmail = new EmailDO();

            //configure the sender information
            var fromEmailAddr = emailAddressRepository.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;
            outboundEmail.From = fromEmailAddr;

            var toEmailAddress = emailAddressRepository.GetOrCreateEmailAddress(attendeeDO.EmailAddress.Address);
            toEmailAddress.Name = attendeeDO.Name;
            outboundEmail.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);

            var user = new User();
            var userID = user.GetOrCreateFromBR(uow, attendeeDO.EmailAddress).Id;
            
            if (isUpdate)
            {

                outboundEmail.Subject = String.Format(ConfigRepository.Get("emailSubjectUpdated"), GetOriginatorName(eventDO), eventDO.Summary, eventDO.StartDate);
                outboundEmail.HTMLText = GetEmailHTMLTextForUpdate(eventDO, userID);
                outboundEmail.PlainText = GetEmailPlainTextForUpdate(eventDO, userID);
            }
            else
            {
                outboundEmail.Subject = String.Format(ConfigRepository.Get("emailSubject"), GetOriginatorName(eventDO), eventDO.Summary, eventDO.StartDate);
                outboundEmail.HTMLText = GetEmailHTMLTextForNew(eventDO, userID);
                outboundEmail.PlainText = GetEmailPlainTextForNew(eventDO, userID);
            }

            //prepare the outbound email
            outboundEmail.EmailStatus = EmailState.Queued;
            if (eventDO.Emails == null)
                eventDO.Emails = new List<EmailDO>();

            eventDO.Emails.Add(outboundEmail);

            uow.EmailRepository.Add(outboundEmail);

            return outboundEmail;
        }
*/


        

        private bool EventHasChanged(IUnitOfWork uow, EventDO eventDO)
        {
            //Stub method for now
            return true;
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

        public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = uow.CommunicationConfigurationRepository;
            foreach (CommunicationConfigurationDO communicationConfig in communicationConfigurationRepo.GetAll().ToList())
            {
                if (communicationConfig.CommunicationType == CommunicationType.Sms)
                {
                    SendBRSMSes(bookingRequests);
                } else if (communicationConfig.CommunicationType == CommunicationType.Email)
                {
                    SendBREmails(communicationConfig.ToAddress, bookingRequests, uow);
                }
                else
                {
                    throw new Exception(String.Format("Invalid communication type '{0}'", communicationConfig.CommunicationType));
                }
            }
            uow.SaveChanges();
        }

        private void SendBRSMSes(IEnumerable<BookingRequestDO> bookingRequests)
        {
            if (bookingRequests.Any())
            {
                var twil = ObjectFactory.GetInstance<ISMSPackager>();
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
                    EmailStatus = EmailState.Queued
                };

                outboundEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress("scheduling@kwasant.com", "Kwasant Scheduling Services");

                outboundEmail.AddEmailRecipient(EmailParticipantType.To, uow.EmailAddressRepository.GetOrCreateEmailAddress(toAddress));

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

    public class RazorViewModel
    {
        public String UserID { get; set; }
        public bool IsAllDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String Summary { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public List<RazorAttendeeViewModel> Attendees { get; set; }

        public RazorViewModel(EventDO ev, String userID)
        {
            IsAllDay = ev.IsAllDay;
            StartDate = ev.StartDate.DateTime;
            EndDate = ev.EndDate.DateTime;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
            Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel { Name = a.Name, EmailAddress = a.EmailAddress.Address }).ToList();
            UserID = userID;
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}