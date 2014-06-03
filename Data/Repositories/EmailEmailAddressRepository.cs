using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class RecipientRepository : GenericRepository<RecipientDO>, IRecipientRepository
    {

        internal RecipientRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IRecipientRepository : IGenericRepository<RecipientDO>
    {
    }
}
