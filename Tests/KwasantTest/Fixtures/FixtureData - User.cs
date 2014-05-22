using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser()
        {
            return new UserDO()
            {
                PersonDO = new PersonDO()
                {
                    FirstName = "Jack",
                    LastName = "Maginot",
                    EmailAddress = TestEmail4()
                }
            };
        }

        public UserDO TestUser2()
        {
            return new UserDO()
            {
                PersonDO = new PersonDO()
                {
                    FirstName = "Rob",
                    LastName = "Maginot",
                    EmailAddress = TestEmail5()
                }
            };
        }

        public UserDO TestImplicitUser()
        {
            return new UserDO()
            {
                PersonDO = new PersonDO()
                {
                    FirstName = "Pabitra"
                },
                UserName = "pabitra1970@gmail.com"
            };
        }
    }
}

