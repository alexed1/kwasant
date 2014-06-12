using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public class ClarificationRequest
    {
        public ClarificationRequestDO Create(IBookingRequest bookingRequest)
        {
            var result = new ClarificationRequestDO()
                       {
                           BookingRequestId = bookingRequest.Id,
                       };
            result.Recipients.Add(new RecipientDO()
                                      {
                                          Email = result, 
                                          EmailAddress = bookingRequest.User.EmailAddress,
                                          EmailAddressID = bookingRequest.User.EmailAddressID,
                                          Type = EmailParticipantType.TO
                                      });
            return result;
        }
    }
}
