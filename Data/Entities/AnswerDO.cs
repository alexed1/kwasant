using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{
    public class AnswerDO : BaseDO, ICreateHook, IModifyHook, IDeleteHook
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

        public void AfterCreate()
        {
            AlertManager.TrackablePropertyCreated("Answer added", "Answer", Id, "Name: " + Text);
        }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var reflectionHelper = new ReflectionHelper<AnswerDO>();

            var textPropertyName = reflectionHelper.GetPropertyName(br => br.Text);
            if (!MiscUtils.AreEqual(originalValues[textPropertyName], currentValues[textPropertyName]))
            {
                AlertManager.TrackablePropertyUpdated("Answer changed", "Answer", Id, Text);
            }
        }

        public void OnDelete(DbPropertyValues originalValues)
        {
            var reflectionHelper = new ReflectionHelper<AnswerDO>();

            var questionIDPropertyName = reflectionHelper.GetPropertyName(br => br.QuestionID);
            AlertManager.TrackablePropertyDeleted("Question deleted", "Question", Id, (int) originalValues[questionIDPropertyName], "Name: " + Text);
        }
    }
}
