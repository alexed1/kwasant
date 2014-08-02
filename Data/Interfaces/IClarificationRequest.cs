using System.Collections.Generic;
using Data.Entities;
using Data.Entities.Constants;

namespace Data.Interfaces
{
    public interface IClarificationRequest : IEmail
    {
        int BookingRequestId { get; set; }
        IBookingRequest BookingRequest { get; set; }
        int ClarificationRequestStateID { get; set; }
        ClarificationRequestStateRow ClarificationRequestState { get; set; }
        IList<QuestionDO> Questions { get; }
    }
}
