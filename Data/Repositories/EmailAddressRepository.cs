using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Interfaces;
using Utilities;

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
            //Validate email here!
            var regexUtil = new RegexUtilities();
            if (!regexUtil.IsValidEmail(email))
                throw new ArgumentException(@"Invalid email format");

            var matchingEmailAddress = UnitOfWork.EmailAddressRepository.DBSet.Local.FirstOrDefault(e => e.Address == email);
            if (matchingEmailAddress == null)
                matchingEmailAddress = UnitOfWork.EmailAddressRepository.GetQuery().FirstOrDefault(e => e.Address == email);

            if (matchingEmailAddress == null)
            {
                matchingEmailAddress = new EmailAddressDO { Address = email };
                UnitOfWork.EmailAddressRepository.Add(matchingEmailAddress);
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
