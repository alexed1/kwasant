using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public EmailAddressDO TestEmailAddress1()
        {

            //password: lucrelucre
            return new EmailAddressDO()
            {
                Address = "alexlucre1@gmail.com", //if this starts failing, might be a gmail capacity limiter. we should just move this to kwasant.net
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
        public EmailAddressDO TestEmailAddress3()
        {

            return new EmailAddressDO()
            {
                Address = "integrationtesting@kwasant.net",
                Id = 3,
                Name = "Kwasant Integration"
            };
        }
      

        public EmailAddressDO TestEmailAddress4()
        {

            return new EmailAddressDO()
            {
                Address = "JackMaginot@gmail.com",
                Id = 4,
                Name = "Jack Test account"
            };
        }

        public EmailAddressDO TestEmailAddress5()
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

