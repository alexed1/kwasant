using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using UtilitiesLib;
using Encoding = System.Text.Encoding;

namespace KwasantCore.Services
{
    public class Event
    {
        private readonly IUnitOfWork _uow;
        public Event()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow = uow; //clean this up finish de-static work
        }

        public void Dispatch(EventDO eventDO)
        {
            var emailAddressRepository = _uow.EmailAddressRepository;

            if (eventDO.Attendees == null)
                eventDO.Attendees = new List<AttendeeDO>();

            string fromEmail = ConfigurationHelper.GetConfigurationValue("fromEmail");
            string fromName = ConfigurationHelper.GetConfigurationValue("fromName");

            EmailDO outboundEmail = new EmailDO();
            var fromEmailAddr = emailAddressRepository.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;

            outboundEmail.AddEmailParticipant(EmailParticipantType.FROM, fromEmailAddr);
            foreach (var attendeeDO in eventDO.Attendees)
            {
                var toEmailAddress = emailAddressRepository.GetOrCreateEmailAddress(attendeeDO.EmailAddress);
                toEmailAddress.Name = attendeeDO.Name;
                outboundEmail.AddEmailParticipant(EmailParticipantType.TO, toEmailAddress);
            }
            outboundEmail.Subject = String.Format(ConfigurationHelper.GetConfigurationValue("emailSubject"), eventDO.Summary, eventDO.StartDate);

            var parsedHTMLEmail = Razor.Parse(Properties.Resources.HTMLEventInvitation, new RazorViewModel(eventDO));
            var parsedPlainEmail = Razor.Parse(Properties.Resources.PlainEventInvitation, new RazorViewModel(eventDO));

            outboundEmail.HTMLText = parsedHTMLEmail;
            outboundEmail.PlainText = parsedPlainEmail;

            outboundEmail.Status = EmailStatus.QUEUED;

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
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

            dDayEvent.Location = eventDO.Location;
            dDayEvent.Description = eventDO.Description;
            dDayEvent.Summary = eventDO.Summary;
            foreach (AttendeeDO attendee in eventDO.Attendees)
            {
                dDayEvent.Attendees.Add(new Attendee
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
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (eventDO.Emails == null)
                eventDO.Emails = new List<EmailDO>();
            eventDO.Emails.Add(outboundEmail);

            new Email(_uow, outboundEmail).Send();
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
        
    }

    public class RazorViewModel
    {
        public bool IsAllDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String Summary { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public List<RazorAttendeeViewModel> Attendees { get; set; }

        public RazorViewModel(EventDO ev)
        {
            IsAllDay = ev.IsAllDay;
            StartDate = ev.StartDate;
            EndDate = ev.EndDate;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
            Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel { Name = a.Name, EmailAddress = a.EmailAddress}).ToList();
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}
