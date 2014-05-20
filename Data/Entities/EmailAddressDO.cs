using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.Repositories;
using StructureMap;

namespace Data.Entities
{
    public class EmailAddressDO : IEmailAddress
    {
        [Key]
        public int Id { get; set; }

        public String Name { get; set; }
        public String Address { get; set; }

        public virtual EmailDO FromEmail { get; set; }
        public virtual EmailDO ToEmail { get; set; }
        public virtual EmailDO BCCEmail { get; set; }
        public virtual EmailDO CCEmail { get; set; }

        public static EmailAddressDO GetOrCreateEmailAddress(String email)
        {
            var emailAddressRepo = new EmailAddressRepository(ObjectFactory.GetInstance<IUnitOfWork>());
            var matchingEmailAddress = emailAddressRepo.GetQuery().FirstOrDefault(e => e.Address == email);
            if (matchingEmailAddress == null)
            {
                matchingEmailAddress = new EmailAddressDO {Address = email};
            }
            return matchingEmailAddress;
        }
    }
}
