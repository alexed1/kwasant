using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PersonRepository : GenericRepository<PersonDO>, IPersonRepository
    {

        internal PersonRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }
    }

    public interface IPersonRepository : IGenericRepository<PersonDO>
    {
      
    }
}
