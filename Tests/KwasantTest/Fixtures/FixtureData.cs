using Data.Interfaces;
using Data.Repositories;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {


        private IUserRepository customerRepo;
        private IEmailAddressRepository emailAddressRepo;
        private IUnitOfWork _uow;

        public FixtureData(IUnitOfWork uow)
        {
            _uow = uow;
            customerRepo = new UserRepository(_uow);
            emailAddressRepo= new EmailAddressRepository(_uow);
        }

    }
}
