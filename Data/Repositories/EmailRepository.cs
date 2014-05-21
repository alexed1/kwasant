using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailRepository : GenericRepository<EmailDO>,  IEmailRepository
    {

        internal EmailRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<EmailDO>
    {
      
    }
}
