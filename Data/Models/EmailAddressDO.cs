using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class EmailAddressDO : IEmailAddress
    {
        [Key]
        public int EmailAddressID { get; set; }

        public String Name { get; set; }
        public String Address { get; set; }

        public virtual EmailDO FromEmailDO { get; set; }
        public virtual EmailDO ToEmailDO { get; set; }
        public virtual EmailDO BccEmailDO { get; set; }
        public virtual EmailDO CcEmailDO { get; set; }
    }
}
