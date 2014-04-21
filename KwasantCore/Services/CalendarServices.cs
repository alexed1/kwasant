using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers;
using Data.Models;
using DDay.DDay.iCal.Components;
using UtilitiesLib;

namespace KwasantCore.Services
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class CalendarServices
    {
        private readonly BookingRequestDO _bookingRequestDO;
        private readonly IUnitOfWork _uow;
        private readonly InvitationRepository _invitationRepo;

        public CalendarServices(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            _uow = uow;
            _bookingRequestDO = bookingRequest;
            _invitationRepo = new InvitationRepository(_uow);
            LoadData();
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _uow;
            }
        }

        private Dictionary<int, InvitationDO> _events;
        public List<InvitationDO> EventsList
        {
            get
            {
                return _events.Values.ToList();
            }
        }

        private void LoadData()
        {
            _events = _invitationRepo.GetQuery()
                .Where(invitationDO => invitationDO.BookingRequest.Customer.CustomerID == _bookingRequestDO.Customer.CustomerID)
                .ToDictionary(
                    invitationDO => invitationDO.InivitationID,
                    invitationDO => invitationDO);

        }

        public void Reload()
        {
            LoadData();
        }

        public void DispatchEvent(InvitationDO invitationDO)
        {
            DispatchEvent(_uow, invitationDO);
            Reload();
        }

        public static void DispatchEvent(IUnitOfWork uow, InvitationDO invitationDO)
        {
            if(invitationDO.Attendees == null)
                invitationDO.Attendees = new List<AttendeeDO>();

            string fromEmail = "lucreorganizer@gmail.com";
            string fromName = "Booqit Organizer";

            EmailDO outboundEmail = new EmailDO();
            outboundEmail.From = new EmailAddressDO {Address = fromEmail, Name = fromName};
            outboundEmail.To = invitationDO.Attendees.Select(a => new EmailAddressDO { Address = a.EmailAddress, Name = a.Name}).ToList();
            outboundEmail.Subject = "Invitation via Booqit: " + invitationDO.Summary + "@ " + invitationDO.StartDate;
            outboundEmail.Text = "This is a Booqit Event Request. For more information, see https://foo.com";
            outboundEmail.StatusID = EmailStatusConstants.QUEUED;

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (invitationDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(invitationDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(invitationDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = invitationDO.Location;
            dDayEvent.Description = invitationDO.Description;
            dDayEvent.Summary = invitationDO.Summary;
            foreach (AttendeeDO attendee in invitationDO.Attendees)
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
                attendee.Invitation = invitationDO;
            }
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (invitationDO.Emails == null)
                invitationDO.Emails = new List<EmailDO>();
            invitationDO.Emails.Add(outboundEmail);

            uow.SaveChanges();
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = EmailServices.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/calendar", Name = "invite.ics" }
                ));

            if(emailDO.Attachments == null)
                emailDO.Attachments = new List<AttachmentDO>();

            emailDO.Attachments.Add(attachmentDO);
        }

        public InvitationDO GetEvent(int eventID)
        {
            return _events[eventID];
        }

        public void AddEvent(InvitationDO invitationDO)
        {
            if (_bookingRequestDO.Invitations == null)
                _bookingRequestDO.Invitations = new List<InvitationDO>();
            _bookingRequestDO.Invitations.Add(invitationDO);

            invitationDO.BookingRequest = _bookingRequestDO;

            _invitationRepo.Add(invitationDO);
            _uow.SaveChanges();
            Reload();
        }

        public void DeleteEvent(int id)
        {
            InvitationDO invitationToDelete = EventsList.FirstOrDefault(inv => inv.InivitationID == id);
            if (invitationToDelete != null && !_invitationRepo.IsDetached(invitationToDelete))
            {
                _invitationRepo.Remove(invitationToDelete);
                _uow.SaveChanges();
            }
            Reload();
        }

        public void DeleteEvent(String idStr)
        {
            int id = idStr.ToInt();
            DeleteEvent(id);
        }

        public void MoveEvent(int id, DateTime newStart, DateTime newEnd)
        {
            InvitationDO itemToMove = EventsList.FirstOrDefault(inv => inv.InivitationID == id);
            if (itemToMove != null)
            {
                itemToMove.StartDate = newStart;
                itemToMove.EndDate = newEnd;
                _uow.SaveChanges();
            }
            Reload();
        }

        public void MoveEvent(String idStr, DateTime newStart, DateTime newEnd)
        {
            int id = Int32.Parse(idStr);
            MoveEvent(id, newStart, newEnd);
        }
    }
}
