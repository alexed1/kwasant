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

        public EmailAddressDO TestEmail3()
        {

            return new EmailAddressDO()
            {
                Address = "pabitra@hotmail.com",
                Id = 3,
                Name = "Person Test account"
            };
        }

        public EmailAddressDO TestEmail4()
        {

            return new EmailAddressDO()
            {
                Address = "JackMaginot@gmail.com",
                Id = 4,
                Name = "Jack Test account"
            };
        }

        public EmailAddressDO TestEmail5()
        {

            return new EmailAddressDO()
            {
                Address = "RobMaginot@gmail.com",
                Id = 5,
                Name = "Jack Test account"
            };
        }
    }
}

