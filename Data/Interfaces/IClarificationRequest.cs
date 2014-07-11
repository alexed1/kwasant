using System;
using System.Collections.Generic;
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
        ClarificationStatus ClarificationStatus { get; set; }
        IList<QuestionDO> Questions { get; } 
    }
}
