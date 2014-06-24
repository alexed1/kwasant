using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Exceptions;
using Utilities;

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
                                                 DateCreated = DateTime.UtcNow,
                                                 DateReceived = DateTime.UtcNow,
                                                 BookingRequestId = bookingRequest.Id,
                                                 Subject = "We need a little more information from you",
                                                 HTMLText = "*** This should be replaced with template body ***",
                                                 PlainText = "*** This should be replaced with template body ***",
                                             };
            String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];
            newClarificationRequestDo.From = Email.GenerateEmailAddress(_uow, new MailAddress(senderMailAddress));
            newClarificationRequestDo.AddEmailRecipient(EmailParticipantType.TO, bookingRequest.User.EmailAddress);
            return newClarificationRequestDo;
        }

        public void Send(ClarificationRequestDO request, string responseUrl)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            var email = new Email(_uow);
            email.SendTemplate("clarification_request_v1", request, new Dictionary<string, string>
                                                                        {
                                                                            { "RESP_URL", responseUrl }
                                                                        });
        }

        public string GenerateResponseURL(ClarificationRequestDO clarificationRequestDO, string responseUrlFormat)
        {
            if (clarificationRequestDO == null)
                throw new ArgumentNullException("clarificationRequestDO");
            var encryptedParams = WebUtility.UrlEncode(Encryption.EncryptParams(new { id = clarificationRequestDO.Id }));
            return string.Format(responseUrlFormat, encryptedParams);
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
