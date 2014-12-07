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

        public override void BeforeSave(IDBContext context)
        {
            base.BeforeSave(context);
            SetBookingRequestLastUpdated(context);
        }
        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues, IDBContext context)
        {
            base.OnModify(originalValues, currentValues, context);
            SetBookingRequestLastUpdated(context);
        }

        public void OnDelete(DbPropertyValues originalValues, IDBContext context)
        {
            SetBookingRequestLastUpdated(context);
        }

        private void SetBookingRequestLastUpdated(IDBContext context)
        {
            using (var uow = new UnitOfWork(context))
            {
                var br = uow.BookingRequestRepository.GetByKey(BookingRequestID);
                br.LastUpdated = DateTime.Now;
            }
        }

    }
}
