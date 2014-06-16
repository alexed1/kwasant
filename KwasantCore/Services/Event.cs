using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Validators;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using Utilities;
using Encoding = System.Text.Encoding;
using IEvent = Data.Interfaces.IEvent;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public EventDO Create(EventDO curEventDO, IUnitOfWork uow)
        {
            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) &&
                                  curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO = AddAttendee(bookingRequestDO.User, curEventDO);
            curEventDO.Status = "Instantiated";

            return curEventDO;
        }

        public EventDO Create(IUnitOfWork uow, int bookingRequestID, string startDate, string endDate)
        {
            var curEventDO = new EventDO();
            curEventDO.StartDate = DateTime.Parse(startDate);
            curEventDO.EndDate = DateTime.Parse(endDate);
            curEventDO.BookingRequestID = bookingRequestID;


            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) &&
                                  curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO = AddAttendee(bookingRequestDO.User, curEventDO);
            curEventDO.Status = "Instantiated";

            return curEventDO;
        }


        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes
        public void Update(IUnitOfWork uow, EventDO eventDO)
        {
            //This line is so that the Server object is compiled. Without this, Razor fails; since it's executed at runtime and the object has been optimized out when running tests.
            var t = Utilities.Server.ServerUrl;
            switch (eventDO.Status)
            {
                case "Instantiated":
                {
                    eventDO.Status = "Undispatched";

                    var calendar = GetCalendarObject(eventDO);
                    foreach (var attendeeDO in eventDO.Attendees)
                    {
                        var emailDO = CreateEmail(uow, eventDO, attendeeDO, false);
                        var email = new Email(uow, emailDO);
                        AttachCalendarToEmail(calendar, emailDO);
                        email.Send();
                    }

                    break;
                }
                case "Dispatched":
                case "Undispatched":
                    //Dispatched means this event was previously created. This is a standard event change. We need to figure out what kind of update message to send
                    //Undispatched is the uncommon case where the event is created, queued up with outbound emails ready to go, and then the event gets changed
                    
                    if (EventHasChanged(uow, eventDO))
                    {
                        eventDO.Status = "Undispatched";
                        var calendar = GetCalendarObject(eventDO);
                        
                        foreach (var attendeeDO in eventDO.Attendees)
                        {
                            //Id > 0 means it's an existing attendee, so we need to send the 'update' email to them.
                            var emailDO = CreateEmail(uow, eventDO, attendeeDO, attendeeDO.Id > 0);
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
                default:
                    throw new Exception("Invalid event status");
            }
        }

        private iCalendar GetCalendarObject(EventDO eventDO)
        {
            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");

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

            //more attendee configuration
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
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };
            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;

            return ddayCalendar;
        }

        private String GetEmailHTMLTextForUpdate(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation_Update, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailPlainTextForUpdate(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation_Update, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailHTMLTextForNew(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailPlainTextForNew(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation, new RazorViewModel(eventDO, userID));
        }

        private EmailDO CreateEmail(IUnitOfWork uow, EventDO eventDO, AttendeeDO attendeeDO, bool isUpdate)
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
            outboundEmail.AddEmailRecipient(EmailParticipantType.TO, toEmailAddress);

            var userID = uow.UserRepository.GetQuery().First(u => u.EmailAddressID == attendeeDO.EmailAddressID).Id;

            if (isUpdate)
            {

                outboundEmail.Subject = GetEmailHTMLTextForUpdate(eventDO, userID);
                outboundEmail.HTMLText = GetEmailPlainTextForUpdate(eventDO, userID);
                outboundEmail.PlainText = String.Format(ConfigRepository.Get("emailSubjectUpdated"), GetOriginatorName(eventDO), eventDO.Summary, eventDO.StartDate);
            }
            else
            {
                outboundEmail.Subject = GetEmailHTMLTextForNew(eventDO, userID);
                outboundEmail.HTMLText = GetEmailPlainTextForNew(eventDO, userID);
                outboundEmail.PlainText = String.Format(ConfigRepository.Get("emailSubject"), GetOriginatorName(eventDO), eventDO.Summary, eventDO.StartDate);
            }

            //prepare the outbound email
            outboundEmail.EmailStatus = EmailStatus.QUEUED;
            if (eventDO.Emails == null)
                eventDO.Emails = new List<EmailDO>();

            eventDO.Emails.Add(outboundEmail);

            return outboundEmail;
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

            if (emailDO.Attachments == null)
                emailDO.Attachments = new List<AttachmentDO>();

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

        private void SendEventUpdates(EventDO curEvent, EventDO newEventData)
        {
            // if eventtimes have changed
            //send an event update email to attendees
        }

        private bool EventHasChanged(IUnitOfWork uow, EventDO eventDO)
        {
            var changedProperties = uow.Db.GetEntityModifications(eventDO);
            //determine if changes warrant an update message
            //return true or false
            return true;
        }


        public EventDO AddAttendee(UserDO curUserDO, EventDO curEvent)
        {
            var curAttendee = new Attendee();
            var curAttendeeDO = curAttendee.Create(curUserDO);
            curEvent.Attendees.Add(curAttendeeDO);
            return curEvent;
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
            StartDate = ev.StartDate;
            EndDate = ev.EndDate;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
            Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel {Name = a.Name, EmailAddress = a.EmailAddress.Address}).ToList();
            UserID = userID;
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}