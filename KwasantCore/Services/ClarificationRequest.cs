using System;
using System.Collections.Generic;
using System.Globalization;
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
            result.AddEmailRecipient(EmailParticipantType.TO, bookingRequest.User.EmailAddress);
            return result;
        }

        public void Send(ClarificationRequestDO request)
        {
            var email = new Email(_uow);
            var encryptedRequestId = request.Id.ToString(CultureInfo.InvariantCulture); // TODO: replace with real utility invoke.
            email.SendTemplate("clarification_request_v1", request, new Dictionary<string, string>() { { "crid", encryptedRequestId } });
        }
    }
}
