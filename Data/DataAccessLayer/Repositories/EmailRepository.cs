using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class EmailRepository : GenericRepository<Email>,  IEmailRepository
    {

        public EmailRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<Email>
    {
      
    }
}
