using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailRepository : GenericRepository<EmailDO>,  IEmailRepository
    {

        public EmailRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<EmailDO>
    {
      
    }
}
