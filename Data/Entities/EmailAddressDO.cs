using System;
using System.Collections.Generic;
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
        [MaxLength(30)]
        public String Address { get; set; }

        public virtual List<EmailEmailAddressDO> EmailEmailAddresses { get; set; }

        public EmailAddressDO()
        {
            EmailEmailAddresses = new List<EmailEmailAddressDO>();
        }

        public static EmailAddressDO GetOrCreateEmailAddress(String email)
        {
            using (var emailAddressRepo = new EmailAddressRepository(ObjectFactory.GetInstance<IUnitOfWork>()))
            {
                var matchingEmailAddress = emailAddressRepo.GetQuery().FirstOrDefault(e => e.Address == email);
                if (matchingEmailAddress == null)
                {
                    matchingEmailAddress = new EmailAddressDO {Address = email};
                }
                return matchingEmailAddress;
            }
        }
    }
}
