using Data.Entities;
using Data.Interfaces;
using System.Net.Mail;
namespace KwasantCore.Services
{
   public class EmailAddress
    {
       public EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address)
       {
           return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
       }
    }
}
