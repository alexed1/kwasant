using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class ClarificationRequestDO : EmailDO, IClarificationRequest
    {
        private IList<QuestionDO> _questions;

        public ClarificationRequestDO()
        {
            _questions = new List<QuestionDO>();
            Calendars = new List<CalendarDO>();
        }

        #region Implementation of IClarificationRequest
        public int BookingRequestId { get; set; }
        IBookingRequest IClarificationRequest.BookingRequest
        {
            get { return BookingRequest; }
            set { BookingRequest = (BookingRequestDO) value; }
        }

        [InverseProperty("ClarficationRequest")]
        public virtual IList<CalendarDO> Calendars { get; set; } 
        public virtual BookingRequestDO BookingRequest { get; set; }
        public ClarificationStatus ClarificationStatus { get; set; }
        public virtual IList<QuestionDO> Questions
        {
            get { return _questions; }
            set { _questions = value; }
        }

        #endregion
    }
}
