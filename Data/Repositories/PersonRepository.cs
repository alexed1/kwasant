using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PersonRepository : GenericRepository<PersonDO>, IPersonRepository
    {

        public PersonRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }

    public interface IPersonRepository : IGenericRepository<PersonDO>
    {
      
    }
}
