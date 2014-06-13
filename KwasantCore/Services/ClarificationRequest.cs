using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Packagers;
using StructureMap;

namespace KwasantCore.Services
{
    public class ClarificationRequest
    {
        private readonly IUnitOfWork _uow;

        public ClarificationRequest(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            _uow = uow;
        }

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

        public void Send(ClarificationRequestDO request)
        {
            var email = new Email(_uow);
            email.SendTemplate("clarification_request_v1", request, new Dictionary<string, string>() { { "crid", "" } }); // TODO: add CR id hash
        }
    }
}
