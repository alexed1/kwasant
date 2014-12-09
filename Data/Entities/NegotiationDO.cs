using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class NegotiationDO : BaseDO, IDeleteHook
    {
        public NegotiationDO()
        {
            Questions = new List<QuestionDO>();
            Attendees = new List<AttendeeDO>();

            NegotiationState = States.NegotiationState.InProcess;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("NegotiationStateTemplate")]
        public int? NegotiationState { get; set; }
        public _NegotiationStateTemplate NegotiationStateTemplate { get; set; }
       
        [ForeignKey("BookingRequest"), Required]
        public int? BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<QuestionDO> Questions { get; set; }

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
            var br = uow.BookingRequestRepository.GetByKey(BookingRequestID);
            if (br != null)
                br.LastUpdated = DateTime.Now;
        }

    }
}
