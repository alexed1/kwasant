using System;
using System.Collections.Generic;
using System.Linq;
using Data.Constants;
using Data.Entities;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {

        public  DDayEvent TestEvent()
        {
            return new DDayEvent()
            {
                
                //DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTStart = new iCalDateTime("20140517"),
                DTEnd = new iCalDateTime("20140610"),
                Location = "San Francisco",
                Description = "First Ever Event",
                Summary = "Here's a Summary",
                WorkflowState = "Undispatched",
                
              //   DateTimeSerializer serializer = new DateTimeSerializer();
            //CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);

            };
        }


        public EventDO TestEvent1()
        {
            return new EventDO()
            {

                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Description = "Description of  Event",
                Priority = 1,
                Sequence = 1,
                IsAllDay = false
            };
        }

        public EventDO TestEvent2()
        {
            return new EventDO()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Priority = 1,
                Sequence = 1,
                IsAllDay = false,
                Category = "Birthday",
                Class = "Private",
                Description = "This is a test event description.",
                Location = "Silicon Valley",
                StateID = EventState.Booking,
                Summary = "This is a test event summary.",
                Transparency = "Opaque" ,
                Attendees = TestAttendeeList1().ToList(),
                CreatedBy = TestUser2()
            };
        }

        public EventDO TestEvent3_TodayDates()
        {
            return new EventDO()
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                Priority = 1,
                Sequence = 1,
                IsAllDay = false,
                Category = "Birthday",
                Class = "Private",
                Description = "This is a test event description.",
                Location = "Silicon Valley",
                StateID = EventState.Booking,
                Summary = "This is a test event summary.",
                Transparency = "Opaque",
                Attendees = TestAttendeeList1().ToList(),
                CreatedBy = TestUser2()
            };
        }

        public EventDO TestEvent4()
        {
            return new EventDO()
            {
                StateID = EventState.Booking,
                Description = "This is my test invitation",
                Summary = @"My test invitation",
                Location = @"Some place!",
                StartDate = DateTime.Today.AddMinutes(5),
                EndDate = DateTime.Today.AddMinutes(15),
                Attendees = TestAttendeeList1().ToList(),
                Emails = new List<EmailDO>(),
                CreatedBy = TestUser2()
            };
        }

    


    }
}
