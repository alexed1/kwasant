using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class StoredFileRepository : GenericRepository<StoredFileDO>, IStoredFileRepository
    {

        internal StoredFileRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IStoredFileRepository : IGenericRepository<StoredFileDO>
    {

    }
}
