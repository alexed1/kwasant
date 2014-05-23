using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class InstructionRepository : GenericRepository<InstructionDO>, IInstructionRepository
    {
        internal InstructionRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IInstructionRepository : IGenericRepository<InstructionDO>
    {
        
    }
}
