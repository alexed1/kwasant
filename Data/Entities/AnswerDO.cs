using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities.Enumerations;

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
        public AnswerStatus Status { get; set; }
        [Required]
        public virtual UserDO User { get; set; }
        public string ObjectsType { get; set; }

        #endregion
    }
}
