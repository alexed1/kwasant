using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers;
using Data.Models;
using DDay.DDay.iCal.Components;

namespace Data.Tools
{
    public static class EventHelper
    {
        public static void DispatchEvent(IUnitOfWork uow, EventDO eventDo)
        {
            EmailRepository emailRepo = new EmailRepository(uow);

            string fromEmail = "lucreorganizer@gmail.com";
            string fromName = "Booqit Organizer";

            MailMessage mailMessage = new MailMessage { From = new MailAddress(fromEmail, fromName) };
            foreach (AttendeeDO attendee in eventDo.Attendees)
                mailMessage.To.Add(new MailAddress(attendee.EmailAddress, attendee.Name));
            mailMessage.Subject = "Invitation via Booqit: " + eventDo.Summary + "@ " + eventDo.StartDate;
            mailMessage.Body = "This is a Booqit Event Request. For more information, see https://foo.com";

            iCalendar calendar = new iCalendar();
            DDayEvent evnt = new DDayEvent();
            if (eventDo.IsAllDay)
            {
                evnt.IsAllDay = true;
            }
            else
            {
                evnt.DTStart = new iCalDateTime(eventDo.StartDate);
                evnt.DTEnd = new iCalDateTime(eventDo.EndDate);
            }
            evnt.DTStamp = new iCalDateTime(DateTime.Now);
            evnt.LastModified = new iCalDateTime(DateTime.Now);

            evnt.Location = eventDo.Location;
            evnt.Description = eventDo.Description;
            evnt.Summary = eventDo.Summary;
            foreach (AttendeeDO attendee in eventDo.Attendees)
            {
                evnt.Attendees.Add(new Attendee
                {
                    CommonName = attendee.Name,
                    Type = "INDIVIDUAL",
                    Role = "REQ-PARTICIPANT",
                    ParticipationStatus = ParticipationStatus.NeedsAction,
                    RSVP = true,
                    Value = new Uri("mailto:" + attendee.EmailAddress),
                });
                attendee.Event = eventDo;
            }
            evnt.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            calendar.Events.Add(evnt);

            EmailDO emailDO = EmailHelper.ConvertMailMessageToEmail(emailRepo, mailMessage);
            AttachCalendarToEmail(calendar, emailDO);
            eventDo.Emails.Add(emailDO);

            uow.SaveChanges();
            EmailHelper.SendEmail(emailDO);
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = EmailHelper.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/calendar", Name = "invite.ics" }
                ));

            emailDO.Attachments.Add(attachmentDO);
        }
    }
}
