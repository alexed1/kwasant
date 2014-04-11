using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class EmailAddress : IEmailAddress
    {
        [Key]
        public int EmailAddressID { get; set; }

        public String Name { get; set; }
        public String Address { get; set; }

        public virtual Email FromEmail { get; set; }
        public virtual Email ToEmail { get; set; }
        public virtual Email BCCEmail { get; set; }
        public virtual Email CCEmail { get; set; }
    }
}
