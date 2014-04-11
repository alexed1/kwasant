using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class EmailStatus : IEmailStatus
    {
        [Key]
        public int EmailStatusID { get; set; }
        public String Value { get; set; }

        public virtual ICollection<Email> Emails { get; set; }
    }
}
