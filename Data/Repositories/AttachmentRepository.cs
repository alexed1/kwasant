using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AttachmentRepository : GenericRepository<AttachmentDO>, IAttachmentRepository
    {
        internal AttachmentRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IAttachmentRepository : IGenericRepository<AttachmentDO>
    {

    }
}
