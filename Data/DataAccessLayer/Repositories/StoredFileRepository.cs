using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{
    public class StoredFileRepository : GenericRepository<StoredFile>, IStoredFileRepository
    {

        public StoredFileRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IStoredFileRepository : IGenericRepository<StoredFile>
    {

    }
}
