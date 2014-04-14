using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{
    public class CustomerRepository : GenericRepository<CustomerDO>, ICustomerRepository
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


