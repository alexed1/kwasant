using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class InstructionRepository : GenericRepository<InstructionDO>, IInstructionRepository
    {
        public InstructionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IInstructionRepository : IGenericRepository<InstructionDO>
    {
        IUnitOfWork UnitOfWork { get; }



    }
}
