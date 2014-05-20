using System;
using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PersonDO : IPerson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        [MaxLength(30, ErrorMessage = "First name maximum 30 characters.")]
        [MinLength(3, ErrorMessage = "First name minimum 3 characters.")]
        public String FirstName { get; set; }

        [Required]
        [StringLength(30)]
        [MaxLength(30, ErrorMessage = "Last name maximum 30 characters.")]
        [MinLength(3, ErrorMessage = "Last name minimum 3 characters.")]        
        public String LastName { get; set; }

    
        public int? EmailAddressID { get; set; }

        [ForeignKey("EmailAddressID")]
        public virtual EmailAddressDO EmailAddress { get; set; }
       
    }
}
