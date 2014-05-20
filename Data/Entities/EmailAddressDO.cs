using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailAddressDO : IEmailAddress
    {
        [Key]
        [Column(Order = 1)] 
        public int Id { get; set; }

        public String Name { get; set; }

        [Key]
        [Column(Order = 2)] 
        public String Address { get; set; }


        [ForeignKey("Id")]
        public virtual PersonDO PersonId { get; set; }

        public virtual EmailDO FromEmail { get; set; }
        public virtual EmailDO ToEmail { get; set; }
        public virtual EmailDO BCCEmail { get; set; }
        public virtual EmailDO CCEmail { get; set; }
    }
}
