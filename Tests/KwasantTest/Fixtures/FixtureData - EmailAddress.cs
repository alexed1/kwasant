using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailAddressDO TestEmailAddress1()
        {

            return new EmailAddressDO()
            {
                Address = "alexlucre1@gmail.com",
                Id = 1,
                Name = "Alex Lucre1"
            };
        }

        public EmailAddressDO TestEmailAddress2()
        {

            return new EmailAddressDO()
            {
                Address = "joetest2@edelstein.org",
                Id = 2,
                Name = "Joe Test Account 2"
            };
        }

    }
}

