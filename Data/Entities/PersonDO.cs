using System;
using System.Runtime.InteropServices.ComTypes;
using Data.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PersonDO : IPerson
    {
        [Key]
        public int Id { get; set; }

        private string _firstName;

        [StringLength(30)]
        [MaxLength(30, ErrorMessage = "First name maximum 30 characters.")]
        [MinLength(3, ErrorMessage = "First name minimum 3 characters.")]
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
                if (EmailAddress != null)
                    EmailAddress.Name = FirstName + " " + LastName;
            }
        }

        private string _lastName;

        [StringLength(30)]
        [MaxLength(30, ErrorMessage = "Last name maximum 30 characters.")]
        [MinLength(3, ErrorMessage = "Last name minimum 3 characters.")]
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value;
                if (EmailAddress != null)
                    EmailAddress.Name = FirstName + " " + LastName;
            }
        }

        private EmailAddressDO _emailAddressDO;

        [Required]
        public EmailAddressDO EmailAddress
        {
            get
            {
                return _emailAddressDO;
            }
            set
            {
                _emailAddressDO = value;
                _emailAddressDO.Name = FirstName + " " + LastName;
            }
        }

        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}
