using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.Enumerations;

namespace Data.Interfaces
{
    public interface IClarificationRequest : IEmail
    {
        int BookingRequestId { get; set; }
        IBookingRequest BookingRequest { get; set; }
        int CRState { get; set; }
        BookingRequestState ClarificationRequestState { get; set; }
        IList<QuestionDO> Questions { get; }
    }
}
