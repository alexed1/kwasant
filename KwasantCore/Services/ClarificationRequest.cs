using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Exceptions;
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
            var newClarificationRequestDo = new ClarificationRequestDO()
                                             {
                                                 DateReceived = DateTime.UtcNow,
                                                 BookingRequestId = bookingRequest.Id,
                                             };
            String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];
            newClarificationRequestDo.From = Email.GenerateEmailAddress(_uow, new MailAddress(senderMailAddress));
            newClarificationRequestDo.AddEmailRecipient(EmailParticipantType.TO, bookingRequest.User.EmailAddress);
            return newClarificationRequestDo;
        }

        public void Send(ClarificationRequestDO request)
        {
            var email = new Email(_uow);
            var encryptedRequestId = request.Id.ToString(CultureInfo.InvariantCulture); // TODO: replace with real utility invoke.
            email.SendTemplate("clarification_request_v1", request, new Dictionary<string, string>() { { "crid", encryptedRequestId } });
        }

        public ClarificationRequestDO GetOrCreateClarificationRequest(IUnitOfWork uow, int bookingRequestId, int clarificationRequestId = 0)
        {
            ClarificationRequestDO clarificationRequest = null;
            if (clarificationRequestId > 0)
            {
                clarificationRequest = uow.ClarificationRequestRepository.GetByKey(clarificationRequestId);
            }
            if (clarificationRequest == null)
            {
                var bookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequest == null)
                    throw new BookingRequestNotFoundException();
                clarificationRequest = Create(bookingRequest);
            }
            return clarificationRequest;
        }

        public void UpdateClarificationRequest(IUnitOfWork uow, IClarificationRequest originalClarificationRequest, IClarificationRequest updatedClarificationRequest)
        {
            foreach (var question in updatedClarificationRequest.Questions)
            {
                originalClarificationRequest.Questions.Add(question);
            }
            uow.SaveChanges();
        }
    }
}
