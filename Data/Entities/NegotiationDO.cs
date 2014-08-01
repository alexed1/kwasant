using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;

namespace Data.Entities
{
    public class NegotiationDO
    {
        #region Implementation of Negotiation

        [Key]
        public int Id { get; set; }

        [ForeignKey("NegotiationState")]
        public int NegotiationStateID { get; set; }
        public NegotiationStateRow NegotiationState { get; set; }
       
        public string Name { get; set; }

        //[ForeignKey("RequestId")]
        //public virtual EmailDO Email { get; set; }

        [ForeignKey("Email"), Required]
        public int RequestId { get; set; }
        public virtual EmailDO Email { get; set; }

        #endregion
    }
}
