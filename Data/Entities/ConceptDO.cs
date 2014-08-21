using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data.Entities
{
    public class ConceptDO
    {
        #region Implementation of Concept

        [Key]
        public int Id { get; set; }
        //public int? ClarificationRequestId { get; set; }
        public int RequestId { get; set; }
        public string Name { get; set; }

        //[ForeignKey("ClarificationRequestId")]
        //public virtual ClarificationRequestDO ClarificationRequest { get; set; }

        [ForeignKey("RequestId")]
        public virtual EmailDO Email { get; set; }

        #endregion
    }
}
