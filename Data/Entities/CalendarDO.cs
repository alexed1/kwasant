using System;
using Data.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class CalendarDO : ICalendar
    {
        [Key]
        public int CalendarId { get; set; }

        [Required]
        [StringLength(300)]
        [MaxLength(300, ErrorMessage = "Calendar name maximum 300 characters.")]
        [MinLength(1, ErrorMessage = "Calendar name minimum 1 character.")]
        public String Name { get; set; }

        public int PersonId { get; set; }       

        [ForeignKey("PersonId")]
        public virtual PersonDO Owner { get; set; }
        
    }
}
