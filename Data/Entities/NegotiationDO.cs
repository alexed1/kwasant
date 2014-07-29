using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class NegotiationDO
    {
        #region Implementation of Negotiation

        [Key]
        public int Id { get; set; }
        public NegotiationState State { get; set; }
        public string Name { get; set; }

        //[ForeignKey("RequestId")]
        //public virtual EmailDO Email { get; set; }

        [ForeignKey("Email"), Required]
        public int RequestId { get; set; }
        public virtual EmailDO Email { get; set; }

        #endregion
    }
}
