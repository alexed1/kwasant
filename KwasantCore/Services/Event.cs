using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.CommunicationManager;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using StructureMap;

namespace KwasantCore.Services
{
    public class Event
    {
        private IUnitOfWork _uow;
        public Event()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow = uow; //clean this up finish de-static work
        }

        public void Dispatch(EventDO curEventDO)
        {
            if (curEventDO.Attendees == null)
                curEventDO.Attendees = new List<AttendeeDO>();



            Email email = new Email(_uow);
      
            string fromEmail = CommunicationManager.GetFromEmail();
            string fromName = CommunicationManager.GetFromName();

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (curEventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(curEventDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(curEventDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = curEventDO.Location;
            dDayEvent.Description = curEventDO.Description;
            dDayEvent.Summary = curEventDO.Summary;
            foreach (AttendeeDO attendee in curEventDO.Attendees)
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
                attendee.Event = curEventDO;
            }
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);

            Calendar curCalendar = new Calendar(_uow);
            EmailDO outboundEmail = email.CreateStandardInviteEmail(curEventDO);
            curCalendar.AttachCalendarToEmail(ddayCalendar, outboundEmail);
            _uow.SaveChanges();


            if (curEventDO.Emails == null)
                curEventDO.Emails = new List<EmailDO>();
            curEventDO.Emails.Add(outboundEmail);

            _uow.SaveChanges();
            //email.Dispatch(outboundEmail); FIX THIS

        }
        
    }
}
