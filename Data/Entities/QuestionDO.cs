using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class QuestionDO : IQuestion
    {
        #region Implementation of IQuestion

        [Key]
        public int Id { get; set; }

        [ForeignKey("QuestionStatusTemplate")]
        public int QuestionStatus { get; set; }
        public _QuestionStatusTemplate QuestionStatusTemplate { get; set; }

        [Required]
        public string Text { get; set; }
        public string AnswerType { get; set; }
        
        public string Response { get; set; }

        [ForeignKey("Calendar")]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }

        [ForeignKey("Negotiation"), Required]
        public int? NegotiationId { get; set; }
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
