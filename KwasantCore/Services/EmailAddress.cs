using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using StructureMap;
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
