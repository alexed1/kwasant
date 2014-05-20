using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Data.Interfaces;

namespace Data.Entities
{
    public class PersonDO : IPerson
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        private string _firstName;
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
