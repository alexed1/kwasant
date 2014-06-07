using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class RecipientRepository : GenericRepository<RecipientDO>, IRecipientRepository
    {

        internal RecipientRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IRecipientRepository : IGenericRepository<RecipientDO>
    {
    }
}
