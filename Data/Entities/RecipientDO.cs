using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;
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

        [ForeignKey("EmailParticipantType")]
        public int EmailParticipantTypeID { get; set; }
        public EmailParticipantType EmailParticipantType { get; set; }
    }
}
