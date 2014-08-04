using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;
using Data.Interfaces;

namespace Data.Entities
{
    public class QuestionDO : IQuestion
    {
        #region Implementation of IQuestion

        [Key]
        public int Id { get; set; }
        
        [ForeignKey("QuestionStatus")]
        public int QuestionStatusID { get; set; }
        public QuestionStatusRow QuestionStatus { get; set; }

        [Required]
        public string Text { get; set; }
        public string AnswerType { get; set; }
        public int NegotiationId { get; set; }
       
        public string Response { get; set; }

      
        [ForeignKey("NegotiationId")]
        public virtual NegotiationDO Negotiation { get; set; }

        [InverseProperty("Question")]
        public virtual List<AnswerDO> Answers { get; set; }
        #endregion


        public QuestionDO()
        {
            Answers = new List<AnswerDO>();
        }
    }
}
