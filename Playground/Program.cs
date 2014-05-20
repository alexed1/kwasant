using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using log4net.Config;
using RazorEngine;
using StructureMap;
using UtilitiesLib;
using UtilitiesLib.Logging;
using Encoding = System.Text.Encoding;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies("dev"); //set to either "test" or "dev"

            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            var evDO = new EventDO()
            {
            };
            evDO.Attendees = new List<AttendeeDO>
            {
                new AttendeeDO
                {
                    EmailAddress = "rjrudman@gmail.com",
                    EventID = evDO.Id,
                    Event = evDO,
                    Name = "Robert Rudman",
               },
               //new AttendeeDO
               // {
               //     EmailAddress = "alex@edelstein.org",
               //     EventID = evDO.EventID,
               //     Event = evDO,
               //     Name = "Alex Edelstein",
               //},
            };
            evDO.Location = "Skype";
            evDO.Description = "Discuss event visualizations";
            evDO.Summary = "Gmail and outlook works, but...";
            evDO.StartDate = DateTime.Now.AddHours(3);
            evDO.EndDate = DateTime.Now.AddHours(4);

            var email = DispatchEvent(unitOfWork, evDO);
            new Email(unitOfWork, email).Send();
        }

        public static EmailDO DispatchEvent(IUnitOfWork uow, EventDO eventDO)
        {
            var emailAddressRepository = new EmailAddressRepository(uow);

            if (eventDO.Attendees == null)
                eventDO.Attendees = new List<AttendeeDO>();

            string fromEmail = ConfigurationHelper.GetConfigurationValue("fromEmail");
            string fromName = ConfigurationHelper.GetConfigurationValue("fromName");

            EmailDO outboundEmail = new EmailDO();
            var fromEmailAddr = EmailAddressDO.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;

            outboundEmail.AddEmailParticipant(EmailParticipantType.FROM, fromEmailAddr);
            foreach (var attendeeDO in eventDO.Attendees)
            {
                var toEmailAddress = EmailAddressDO.GetOrCreateEmailAddress(attendeeDO.EmailAddress);
                emailAddressRepository.Attach(toEmailAddress);
                toEmailAddress.Name = attendeeDO.Name;
                outboundEmail.AddEmailParticipant(EmailParticipantType.TO, toEmailAddress);
            }
            outboundEmail.Subject = String.Format(ConfigurationHelper.GetConfigurationValue("emailSubject"), eventDO.Summary, eventDO.StartDate);

            var parsedHTMLEmail = Razor.Parse(KwasantCore.Properties.Resources.HTMLEventInvitation, eventDO);
            var parsedPlainEmail = Razor.Parse(KwasantCore.Properties.Resources.PlainEventInvitation, eventDO);

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

            return outboundEmail;
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
                    ) {TransferEncoding = TransferEncoding.Base64});
        }
    }
}
