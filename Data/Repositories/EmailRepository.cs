using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailRepository : GenericRepository<EmailDO>,  IEmailRepository
    {

        internal EmailRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<EmailDO>
    {
      
    }
}
