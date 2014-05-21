using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EmailAddressRepository : GenericRepository<EmailAddressDO>,  IEmailAddressRepository
    {
        internal EmailAddressRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }

        public EmailAddressDO GetOrCreateEmailAddress(String email)
        {
            var matchingEmailAddress = _dbContext.UnitOfWork.EmailAddressRepository.GetQuery().FirstOrDefault(e => e.Address == email);
            if (matchingEmailAddress == null)
            {
                matchingEmailAddress = new EmailAddressDO { Address = email };
            }
            return matchingEmailAddress;
        }
    }

    public interface IEmailAddressRepository : IGenericRepository<EmailAddressDO>
    {
        EmailAddressDO GetOrCreateEmailAddress(String email);
    }
}
