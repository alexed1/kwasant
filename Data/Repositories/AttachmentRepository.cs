using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AttachmentRepository : GenericRepository<AttachmentDO>, IAttachmentRepository
    {
        internal AttachmentRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IAttachmentRepository : IGenericRepository<AttachmentDO>
    {

    }
}
