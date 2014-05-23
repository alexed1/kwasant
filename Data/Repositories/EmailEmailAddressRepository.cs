using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EmailEmailAddressRepository : GenericRepository<EmailEmailAddressDO>, IEmailEmailAddressRepository
    {

        internal EmailEmailAddressRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IEmailEmailAddressRepository : IGenericRepository<EmailEmailAddressDO>
    {
    }
}
