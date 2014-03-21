using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {


        public ICustomerRepository customerRepo;
        public IUnitOfWork _uow;

        public FixtureData(IUnitOfWork uow)
        {
            _uow = uow;
            customerRepo = new CustomerRepository(_uow);
        }

    }
}
