using Data.DataAccessLayer.Interfaces;
using Data.Interfaces;
using Data.Repositories;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {


        private ICustomerRepository customerRepo;
        private IEmailAddressRepository emailAddressRepo;
        private IUnitOfWork _uow;

        public FixtureData(IUnitOfWork uow)
        {
            _uow = uow;
            customerRepo = new CustomerRepository(_uow);
            emailAddressRepo= new EmailAddressRepository(_uow);
        }

    }
}
