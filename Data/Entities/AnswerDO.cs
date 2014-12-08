using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class AnswerDO : BaseDO, IDeleteHook
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
            var br = uow.BookingRequestRepository.GetByKey(Question.Negotiation.BookingRequestID);
            br.LastUpdated = DateTime.Now;
        }
    }
}
