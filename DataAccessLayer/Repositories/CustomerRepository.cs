using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

        public void Update()
        {
        }
    }
}


