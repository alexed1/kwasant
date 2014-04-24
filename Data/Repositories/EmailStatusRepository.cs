using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EmailStatusRepository : GenericRepository<EmailStatusDO>, IEmailStatusRepository
    {
        public EmailStatusRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IEmailStatusRepository : IGenericRepository<EmailStatusDO>
    {
        IUnitOfWork UnitOfWork { get; }



    }
}
