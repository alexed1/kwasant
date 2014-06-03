using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class StoredFileRepository : GenericRepository<StoredFileDO>, IStoredFileRepository
    {

        internal StoredFileRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IStoredFileRepository : IGenericRepository<StoredFileDO>
    {

    }
}
