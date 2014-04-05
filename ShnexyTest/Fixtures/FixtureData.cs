using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;

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
