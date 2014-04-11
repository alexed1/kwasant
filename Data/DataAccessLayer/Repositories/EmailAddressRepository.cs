using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class EmailAddressRepository : GenericRepository<EmailAddress>,  IEmailAddressRepository
    {

        public EmailAddressRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailAddressRepository : IGenericRepository<EmailAddress>
    {
        IUnitOfWork UnitOfWork { get; }

      
   
    }
}
