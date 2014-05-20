using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailEmailAddressRepository : GenericRepository<EmailEmailAddressDO>, IEmailEmailAddressRepository
    {

        public EmailEmailAddressRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IEmailEmailAddressRepository : IGenericRepository<EmailEmailAddressDO>
    {
        IUnitOfWork UnitOfWork { get; }



    }
}
