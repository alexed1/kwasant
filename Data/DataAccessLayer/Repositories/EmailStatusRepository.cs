using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
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
