using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser1()
        {
            return new UserDO()
            {                
                FirstName = "Jack",
                LastName = "Maginot",
                EmailAddress = TestEmailAddress1() //note that some tests have Attendees that assume this won't change
            };
        }

        public UserDO TestUser2()
        {
            return new UserDO()
            {
                FirstName = "Rob",
                LastName = "Maginot",
                EmailAddress = TestEmailAddress5()
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
                EmailAddressID = curEmailAddressDO.Id
            };
        }
    }
}

