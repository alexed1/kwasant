using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class EmailAddressRepository : GenericRepository<Email>,  IEmailAddressRepository
    {

        public EmailAddressRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailAddressRepository : IGenericRepository<Email>
    {
        IUnitOfWork UnitOfWork { get; }

      
   
    }
}
