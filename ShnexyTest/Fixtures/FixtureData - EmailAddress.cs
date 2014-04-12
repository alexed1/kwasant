using Data.Models;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailAddress TestEmail1()
        {

            return new EmailAddress()
            {
                Address = "alexlucre1@gmail.com",
                EmailAddressID = 1,
                Name = "Alex Lucre1"
            };
        }

        public EmailAddress TestEmail2()
        {

            return new EmailAddress()
            {
                Address = "joetest2@edelstein.org",
                EmailAddressID = 2,
                Name = "Joe Test Account 2"
            };
        }

    }
}

