using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class RecipientRepository : GenericRepository<Recipient>, IRecipientRepository
    {

        internal RecipientRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IRecipientRepository : IGenericRepository<Recipient>
    {
    }
}
