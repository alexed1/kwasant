using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{
    public class EmailStatusRepository : GenericRepository<EmailStatus>, IEmailStatusRepository
    {
        public EmailStatusRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IEmailStatusRepository : IGenericRepository<EmailStatus>
    {
        IUnitOfWork UnitOfWork { get; }



    }
}
