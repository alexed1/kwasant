using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
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
