using System;
using System.Linq;
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
                Status = "Open",
                Summary = "This is a test event summary.",
                Transparency = "Opaque" ,
                Attendees = TestAttendeeList1().ToList()
            };
        }



    }
}
