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

        public Email FromEmail { get; set; }
        public Email ToEmail { get; set; }
        public Email BCCEmail { get; set; }
        public Email CCEmail { get; set; }
    }
}
