using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class QuestionResponseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public virtual QuestionDO Question { get; set; }

        [ForeignKey("Answer")]
        public int? AnswerID { get; set; }
        public virtual AnswerDO Answer { get; set; }

        [ForeignKey("User")]
        public String UserID { get; set; }
        public virtual UserDO User { get; set; }


        public String Text { get; set; }

        [ForeignKey("Calendar")]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }
    }
}
