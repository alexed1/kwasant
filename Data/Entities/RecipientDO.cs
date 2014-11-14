﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class RecipientDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email"), Required]
        public int? EmailID { get; set; }
        public virtual EmailDO Email { get; set; }

        [ForeignKey("EmailAddress"), Required]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [ForeignKey("EmailParticipantTypeTemplate")]
        public int? EmailParticipantType { get; set; }
        public _EmailParticipantTypeTemplate EmailParticipantTypeTemplate { get; set; }
    }
}
