using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class StoredFileRepository : GenericRepository<StoredFileDO>, IStoredFileRepository
    {

        internal StoredFileRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IStoredFileRepository : IGenericRepository<StoredFileDO>
    {

    }
}
