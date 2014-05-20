using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser
    {
        public UserDO()
        {
            PersonDO = new PersonDO();
        }
        
        public UserDO(string curEmailAddress) : this()
        {
            EmailAddressDO = EmailAddressDO.GetOrCreateEmailAddress(curEmailAddress);
        }

        [NotMapped]
        public string FirstName
        {
            get { return PersonDO.FirstName; }
            set { PersonDO.FirstName = value; }
        }

        [NotMapped]        
        public string LastName
        {
            get { return PersonDO.LastName; }
            set { PersonDO.LastName = value; }
        }

        [NotMapped]
        public EmailAddressDO EmailAddressDO
        {
            get { return PersonDO.EmailAddress; }
            set { PersonDO.EmailAddress = value; }
        }

        /// <summary>
        /// This property may not be required a base class has property called PasswordHash 
        /// where password is stored in encrypted form and decrypted when it is fetched.
        /// </summary>
        [NotMapped]
        public string Password
        {
            get { return base.PasswordHash; }
            set { base.PasswordHash = value; }
        }

        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }

        [ForeignKey("PersonDO")]
        public int PersonID { get; set; }
        [Required]
        public virtual PersonDO PersonDO { get; set; }
    }
}
