using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class InstructionRepository : GenericRepository<InstructionDO>, IInstructionRepository
    {
        internal InstructionRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IInstructionRepository : IGenericRepository<InstructionDO>
    {
        
    }
}
