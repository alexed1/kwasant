using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser()
        {
            return new UserDO()
            {                
                FirstName = "Jack",
                LastName = "Maginot",
                EmailAddress = TestEmail4()
            };
        }

        public UserDO TestUser2()
        {
            return new UserDO()
            {
                FirstName = "Rob",
                LastName = "Maginot",
                EmailAddress = TestEmail5()
            };
        }

        public UserDO TestImplicitUser()
        {
            return new UserDO()
            {
                FirstName = "Pabitra",
                UserName = "pabitra1970@gmail.com"
            };
        }
    }
}

