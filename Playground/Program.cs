using System;
using Daemons;
using Data.Entities;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using Utilities;

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
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "dev"

            new ThroughputMonitor().Start();

            //var evdo = new EventDO();
            //evdo.StartDate = DateTime.Now;
            //evdo.EndDate = DateTime.Now.AddHours(1);
            //evdo.Attendees = new List<AttendeeDO>();

            //var iCal = GetCalendarObject(evdo);
            //iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            //string fileToAttach = serializer.Serialize(iCal);
        }



        private static iCalendar GetCalendarObject(EventDO eventDO)
        {
            IConfigRepository configRepository = new ConfigRepository();
            string fromEmail = configRepository.Get("fromEmail");
            string fromName = configRepository.Get("fromName");

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();

            //configure start and end time
            if (eventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(eventDO.StartDate.ToUniversalTime().DateTime);
                dDayEvent.DTEnd = new iCalDateTime(eventDO.EndDate.ToUniversalTime().DateTime);
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

    }
}
