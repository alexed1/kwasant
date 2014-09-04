using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            return new UserDO()
            {       
                Id = "1",
                FirstName = "Jack",
                LastName = "Maginot",
                EmailAddressID = curEmailAddressDO.Id,
                EmailAddress = curEmailAddressDO //note that some tests have Attendees that assume this won't change
            };
        }

        public UserDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            return new UserDO()
            {
                FirstName = "Rob",
                LastName = "Maginot",
                EmailAddressID = curEmailAddressDO.Id,
                EmailAddress = curEmailAddressDO
            };
        }

        public UserDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            return new UserDO()
            {
                FirstName = "Kwasant",
                LastName = "Integration",
                EmailAddress = curEmailAddressDO,
                EmailAddressID = curEmailAddressDO.Id,
                TestAccount = true
            };
        }
    }
}

