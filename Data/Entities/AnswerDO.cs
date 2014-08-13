using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class AnswerDO
    {
        #region Implementation of Answer

        [Key]
        public int Id { get; set; }
        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public virtual QuestionDO Question { get; set; }

        [ForeignKey("Calendar")]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }

        [ForeignKey("AnswerStatusTemplate")]
        public int AnswerStatus { get; set; }
        public _AnswerStatusTemplate AnswerStatusTemplate { get; set; }
        
        public virtual UserDO User { get; set; }
        public string ObjectsType { get; set; }
        public string Text { get; set; }

        
        #endregion
    }
}
