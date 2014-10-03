using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public UserDO TestUser1()
        {
            var curEmailAddressDO = TestEmailAddress1();
            var existing = _uow.UserRepository.GetByEmailAddress(curEmailAddressDO);
            if (existing != null)
                return existing;
            var newUser = new UserDO()
            {       
                Id = "1",
                FirstName = "Jack",
                LastName = "Maginot",
                EmailAddressID = curEmailAddressDO.Id,
                EmailAddress = curEmailAddressDO //note that some tests have Attendees that assume this won't change
            };
            _uow.UserRepository.Add(newUser);
            return newUser;
        }

        public UserDO TestUser2()
        {
            var curEmailAddressDO = TestEmailAddress5();
            var existing = _uow.UserRepository.GetByEmailAddress(curEmailAddressDO);
            if (existing != null)
                return existing;
            var newUser = new UserDO()
            {
                FirstName = "Rob",
                LastName = "Maginot",
                EmailAddressID = curEmailAddressDO.Id,
                EmailAddress = curEmailAddressDO
            };
            _uow.UserRepository.Add(newUser);
            return newUser;
        }

        public UserDO TestUser3()
        {
            var curEmailAddressDO = TestEmailAddress3();
            var existing = _uow.UserRepository.GetByEmailAddress(curEmailAddressDO);
            if (existing != null)
                return existing;
            var newUser = new UserDO()
            {
                FirstName = "Kwasant",
                LastName = "Integration",
                EmailAddress = curEmailAddressDO,
                EmailAddressID = curEmailAddressDO.Id,
                TestAccount = true
            };
            _uow.UserRepository.Add(newUser);
            return newUser;
        }
    }
}

