using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{

    public class QuestionDO : BaseDO, IQuestionDO, ICreateHook, IModifyHook, IDeleteHook
    {
        public QuestionDO()
        {
            Answers = new List<AnswerDO>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }
        public string AnswerType { get; set; }
        public string Response { get; set; }

        [ForeignKey("QuestionStatusTemplate")]
        public int? QuestionStatus { get; set; }
        public _QuestionStatusTemplate QuestionStatusTemplate { get; set; }

        [ForeignKey("Calendar")]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }

        [ForeignKey("Negotiation"), Required]
        public int? NegotiationId { get; set; }
        public virtual NegotiationDO Negotiation { get; set; }

        [InverseProperty("Question")]
        public virtual List<AnswerDO> Answers { get; set; }

        public override void AfterCreate()
        {
            AlertManager.TrackablePropertyCreated("Question added", "Question", Id, "Name: " + Text);
            base.AfterCreate();
        }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var reflectionHelper = new ReflectionHelper<QuestionDO>();

            var textPropertyName = reflectionHelper.GetPropertyName(br => br.Text);
            if (!MiscUtils.AreEqual(originalValues[textPropertyName], currentValues[textPropertyName]))
            {
                AlertManager.TrackablePropertyUpdated("Question changed", "Question", Id, Text);
}
        }

        public void OnDelete(DbPropertyValues originalValues)
        {
            var reflectionHelper = new ReflectionHelper<QuestionDO>();

            var negotiationIDPropertyName = reflectionHelper.GetPropertyName(br => br.NegotiationId);
            AlertManager.TrackablePropertyDeleted("Question deleted", "Question", Id, (int)originalValues[negotiationIDPropertyName], "Name: " + Text);
        }
    }
}
