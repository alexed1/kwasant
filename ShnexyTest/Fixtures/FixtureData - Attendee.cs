using Data.Models;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {





        public Attendee TestAttendee1()
        {

            return new Attendee()
            {
                EmailAddress = "alexlucre1@gmail.com",
                Name = "Alex Lucre1"
            };
        }

        public Attendee TestAttendee2()
        {

            return new Attendee()
            {
                EmailAddress = "joetest2@edelstein.org",
                Name = "Joe Test Account 2"
            };
        }

    }
}

