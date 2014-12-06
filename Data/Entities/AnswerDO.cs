using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{
    public class AnswerDO : BaseDO, ICreateHook, IDeleteHook
    {
        [Key]
        public int Id { get; set; }

        public string Text { get; set; }

        [ForeignKey("Question")]
        public int? QuestionID { get; set; }
        public virtual QuestionDO Question { get; set; }

        [ForeignKey("Event")]
        public int? EventID { get; set; }
        public virtual EventDO Event { get; set; }

        [ForeignKey("AnswerStatusTemplate")]
        public int? AnswerStatus { get; set; }
        public _AnswerStatusTemplate AnswerStatusTemplate { get; set; }

        [ForeignKey("UserDO")]
        public string UserID { get; set; }
        public virtual UserDO UserDO { get; set; }

        public void OnDelete(DbPropertyValues originalValues)
        {

        }
    }
}
