using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class NegotiationDO
    {
        #region Implementation of Negotiation

        [Key]
        public int Id { get; set; }
        //public int? ClarificationRequestId { get; set; }
        public int RequestId { get; set; }
        public string State { get; set; }
        public string Name { get; set; }

        //[ForeignKey("ClarificationRequestId")]
        //public virtual ClarificationRequestDO ClarificationRequest { get; set; }

        [ForeignKey("RequestId")]
        public virtual EmailDO Email { get; set; }

        #endregion
    }
}
