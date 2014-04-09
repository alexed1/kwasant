using Data.Models;

namespace Data.DataAccessLayer.Fixtures
{
    public partial class FixtureData
    {




   
        public EmailAddress TestEmailAddress1()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "alexlucre1@gmail.com",
                Id = 1,
                Name = "Alex Lucre1"
            };
        }

        public EmailAddress TestEmailAddress2()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "joetest2@edelstein.org",
                Id = 2,
                Name = "Joe Test Account 2"
            };
        }

        public EmailAddress EmailAddress_KwasantHQ()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "Organizer@kwsasnt.com",
                Id = 2,
                Name = "Kwasant Organizer Account"
            };
        }

    }
}

