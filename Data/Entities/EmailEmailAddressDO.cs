using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Enumerations;

namespace Data.Entities
{
    public class EmailEmailAddressDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email")]
        public int EmailID { get; set; }
        public EmailDO Email { get; set; }

        [ForeignKey("EmailAddress")]
        public int EmailAddressID { get; set; }
        public EmailAddressDO EmailAddress { get; set; }

        public EmailParticipantType Type { get; set; }
    }
}
