using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;
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
        
        [Required, ForeignKey("ClarificationRequestState")]
        public int ClarificationRequestStateID { get; set; }
        public ClarificationRequestStateRow ClarificationRequestState { get; set; }

        public virtual IList<QuestionDO> Questions
        {
            get { return _questions; }
            set { _questions = value; }
        }

        public int NegotiationId { get; set; }
        [ForeignKey("NegotiationId")]
        public virtual NegotiationDO Negotiation { get; set; }

        //should add a setter override that prevents more than one recipient being added to the To of a CR.

        #endregion
    }
}
