using Data.Models;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailAddress TestEmail1()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "alexlucre1@gmail.com",
                Id = 1,
                Name = "Alex Lucre1"
            };
        }

        public EmailAddress TestEmail2()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "joetest2@edelstein.org",
                Id = 2,
                Name = "Joe Test Account 2"
            };
        }

    }
}

