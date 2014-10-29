using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailAddressDO : BaseDO, IEmailAddressDO
    {
        public EmailAddressDO()
        {
            Recipients = new List<RecipientDO>();
        }

        public EmailAddressDO(string emailAddress)
        {
            Recipients = new List<RecipientDO>();
            Address = emailAddress;
        }

        [Key]
        public int Id { get; set; }

        public String Name { get; set; }
        [MaxLength(256)]
        public String Address { get; set; }

        [InverseProperty("EmailAddress")]
        public virtual List<RecipientDO> Recipients { get; set; }
    }
}
