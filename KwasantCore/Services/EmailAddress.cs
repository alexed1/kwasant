using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Net.Mail;
namespace KwasantCore.Services
{
    public class EmailAddress
    {
        public EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address)
        {
            return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
        }

        public EmailAddressDO ConvertFromString(string emailString, IUnitOfWork uow)
        {
            String email = string.Empty;
            String name = string.Empty;
            emailString = emailString.Replace("\"", string.Empty);
            if (emailString.Contains("<"))
            {
                string[] parts = emailString.Split('<');
                name = parts[0];
                email = parts[1];
                email = email.Replace(">", string.Empty);
            }
            else
                email = emailString;

            //using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            EmailAddressDO convertAddresFromString = uow.EmailAddressRepository.GetOrCreateEmailAddress(email, name);
            return convertAddresFromString;
           //}
        }

    }
}
