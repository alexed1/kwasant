using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EmailAddressRepository : GenericRepository<EmailAddressDO>,  IEmailAddressRepository
    {
        internal EmailAddressRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

        public EmailAddressDO GetOrCreateEmailAddress(String email, String name = null)
        {
            var matchingEmailAddress = UnitOfWork.EmailAddressRepository.DBSet.Local.FirstOrDefault(e => e.Address == email);
            if (matchingEmailAddress == null)
                matchingEmailAddress = UnitOfWork.EmailAddressRepository.GetQuery().FirstOrDefault(e => e.Address == email);

            if (matchingEmailAddress == null)
            {
                matchingEmailAddress = new EmailAddressDO { Address = email };
                UnitOfWork.EmailAddressRepository.Add(matchingEmailAddress);

                //we automatically create a new "implicit" User object for each email address we learn about.
                UserDO newUser = new UserDO();
                newUser.EmailAddress = matchingEmailAddress;
                newUser.UserName = matchingEmailAddress.Address;
                newUser.FirstName = name;
                UnitOfWork.UserRepository.Add(newUser);
            }
            if(!String.IsNullOrEmpty(name))
                matchingEmailAddress.Name = name;
            return matchingEmailAddress;
        }
    }

    public interface IEmailAddressRepository : IGenericRepository<EmailAddressDO>
    {
        EmailAddressDO GetOrCreateEmailAddress(String email, String name);
    }
}
