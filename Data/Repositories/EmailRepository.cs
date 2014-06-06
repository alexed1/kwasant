using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailRepository : GenericRepository<EmailDO>,  IEmailRepository
    {

        internal EmailRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<EmailDO>
    {
      
    }
}
