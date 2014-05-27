using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Data.Interfaces;
using StructureMap;

namespace Data.Entities
{
    public sealed class EmailAddressDO : IEmailAddress
    {
        [Key]
        public int Id { get; set; }

        public String Name { get; set; }
        [MaxLength(30)]
        public String Address { get; set; }

        public List<Recipient> Recipients { get; set; }

        public EmailAddressDO()
        {
            Recipients = new List<Recipient>();
        }
    }
}
