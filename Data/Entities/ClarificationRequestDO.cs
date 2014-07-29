using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        #region Implementation of IClarificationRequest
        public int BookingRequestId { get; set; }
        IBookingRequest IClarificationRequest.BookingRequest
        {
            get { return BookingRequest; }
            set { BookingRequest = (BookingRequestDO) value; }
        }
        public virtual BookingRequestDO BookingRequest { get; set; }
        [ForeignKey("ClarificationRequestState")]
        [Required]
        public int CRState { get; set; }
        public virtual BookingRequestState ClarificationRequestState { get; set; }
        public virtual IList<QuestionDO> Questions
        {
            get { return _questions; }
            set { _questions = value; }
        }

        #endregion
    }
}
