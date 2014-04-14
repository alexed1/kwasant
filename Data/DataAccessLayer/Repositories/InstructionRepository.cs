using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
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
