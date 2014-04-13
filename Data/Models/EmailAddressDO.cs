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

        public virtual EmailDO FromEmail { get; set; }
        public virtual EmailDO ToEmail { get; set; }
        public virtual EmailDO BCCEmail { get; set; }
        public virtual EmailDO CCEmail { get; set; }
    }
}
