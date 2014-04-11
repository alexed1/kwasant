using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{
    public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
    {

        public AttachmentRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IAttachmentRepository : IGenericRepository<Attachment>
    {

    }
}
