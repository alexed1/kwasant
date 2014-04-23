using Data.Models;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {





        public AttendeeDO TestAttendee1()
        {

            return new AttendeeDO()
            {
                EmailAddress = "alexlucre1@gmail.com",
                Name = "Alex Lucre1"
            };
        }

        public AttendeeDO TestAttendee2()
        {

            return new AttendeeDO()
            {
                EmailAddress = "joetest2@edelstein.org",
                Name = "Joe Test Account 2"
            };
        }

    }
}

