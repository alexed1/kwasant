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

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EmailAddressDO EmailAddress { get; set; }
        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}
