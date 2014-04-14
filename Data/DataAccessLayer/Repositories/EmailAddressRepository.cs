using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class EmailAddressRepository : GenericRepository<EmailAddressDO>,  IEmailAddressRepository
    {

        public EmailAddressRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailAddressRepository : IGenericRepository<EmailAddressDO>
    {
        IUnitOfWork UnitOfWork { get; }

      
   
    }
}
