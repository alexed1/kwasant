using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class QuestionResponseDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Answer")]
        public int? AnswerID { get; set; }
        public virtual AnswerDO Answer { get; set; }

        [ForeignKey("User")]
        public String UserID { get; set; }
        public virtual UserDO User { get; set; }
    }
}
