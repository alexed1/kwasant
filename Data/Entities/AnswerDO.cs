using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Constants;
using Data.Entities.Constants;

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

        [ForeignKey("AnswerStatus")]
        public int AnswerStatusID { get; set; }
        public AnswerStatusRow AnswerStatus { get; set; }
        
        [Required]
        public virtual UserDO User { get; set; }
        public string ObjectsType { get; set; }

        #endregion
    }
}
