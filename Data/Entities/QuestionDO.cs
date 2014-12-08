using System;
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

    public class QuestionDO : BaseDO, IQuestionDO, IDeleteHook
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


        public override void BeforeSave(IUnitOfWork uow)
        {
            base.BeforeSave(uow);
            SetBookingRequestLastUpdated(uow);
        }
        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues, IUnitOfWork uow)
        {
            base.OnModify(originalValues, currentValues, uow);
            SetBookingRequestLastUpdated(uow);
        }

        public void OnDelete(DbPropertyValues originalValues, IUnitOfWork uow)
        {
            SetBookingRequestLastUpdated(uow);
        }

        private void SetBookingRequestLastUpdated(IUnitOfWork uow)
        {
            if (Negotiation != null)
            {
                var br = uow.BookingRequestRepository.GetByKey(Negotiation.BookingRequestID);
                if (br != null)
                    br.LastUpdated = DateTime.Now;
            }
        }
    }
}
