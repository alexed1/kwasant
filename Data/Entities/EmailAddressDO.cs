using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailAddressDO : IEmailAddress
    {
        [Key]
        public int EmailAddressID { get; set; }

        public String Name { get; set; }
        public String Address { get; set; }

        public virtual List<EmailEmailAddressDO> EmailEmailAddresses { get; set; }

        public EmailAddressDO()
        {
            EmailEmailAddresses = new List<EmailEmailAddressDO>();
        }
    }
}
