using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Enumerations;

namespace Data.Entities
{
    public class RecipientDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email")]
        public int EmailID { get; set; }
        public virtual EmailDO Email { get; set; }

        [ForeignKey("EmailAddress")]
        public int EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        public EmailParticipantType Type { get; set; }
    }
}
