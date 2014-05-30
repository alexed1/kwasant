using System.Collections.Generic;
using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {





        public AttendeeDO TestAttendee1()
        {

            return new AttendeeDO()
            {
                EmailAddress = TestEmailAddress1(),
                Name = "Alex Lucre1"
            };
        }

        public AttendeeDO TestAttendee2()
        {

            return new AttendeeDO()
            {
                EmailAddress = TestEmailAddress2(),
                Name = "Joe Test Account 2"
            };
        }

        public IEnumerable<AttendeeDO> TestAttendeeList1()
        {
            List<AttendeeDO> attendeeList = new List<AttendeeDO>();
            AttendeeDO attendee = TestAttendee1();
            attendeeList.Add(attendee);
            return attendeeList;
        }

    }
}

