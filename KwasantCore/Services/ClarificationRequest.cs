using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Exceptions;
using Utilities;
using StructureMap;

namespace KwasantCore.Services
{
    public class ClarificationRequest
    {
        public ClarificationRequestDO Create(IUnitOfWork uow, IBookingRequest bookingRequest)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (bookingRequest == null)
                throw new ArgumentNullException("bookingRequest");
            var newClarificationRequestDo = new ClarificationRequestDO()
                                             {
                                                 DateCreated = DateTime.UtcNow,
                                                 DateReceived = DateTime.UtcNow,
                                                 BookingRequestId = bookingRequest.Id,
                                                 Subject = "We need a little more information from you",
                                                 HTMLText = "*** This should be replaced with template body ***",
                                                 PlainText = "*** This should be replaced with template body ***",
                                             };
            ((IClarificationRequest) newClarificationRequestDo).BookingRequest = bookingRequest;
            String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];
            newClarificationRequestDo.From = Email.GenerateEmailAddress(uow, new MailAddress(senderMailAddress));
            newClarificationRequestDo.AddEmailRecipient(EmailParticipantType.TO, bookingRequest.User.EmailAddress);
            return newClarificationRequestDo;
        }

        public void Send(IUnitOfWork uow, IClarificationRequest request, string responseUrl)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (request == null)
                throw new ArgumentNullException("request");
            var email = new Email(uow);
            email.SendTemplate("clarification_request_v1", request, new Dictionary<string, string>
                                                                        {
                                                                            { "RESP_URL", responseUrl }
                                                                        });
        }

        public string GenerateResponseURL(IClarificationRequest clarificationRequestDO, string responseUrlFormat)
        {
            if (clarificationRequestDO == null)
                throw new ArgumentNullException("clarificationRequestDO");
            var encryptedParams = WebUtility.UrlEncode(Encryption.EncryptParams(new { id = clarificationRequestDO.Id }));
            return string.Format(responseUrlFormat, encryptedParams);
        }

        public IClarificationRequest GetOrCreateClarificationRequest(IUnitOfWork uow, int bookingRequestId, int clarificationRequestId = 0)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            IClarificationRequest clarificationRequest = null;
            if (clarificationRequestId > 0)
            {
                clarificationRequest = uow.ClarificationRequestRepository.GetByKey(clarificationRequestId);
            }
            if (clarificationRequest == null)
            {
                var bookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequest == null)
                    throw new EntityNotFoundException<IBookingRequest>();
                clarificationRequest = Create(uow, bookingRequest);
            }
            return clarificationRequest;
        }

        public void UpdateClarificationRequest(IUnitOfWork uow, IClarificationRequest originalClarificationRequest, IClarificationRequest updatedClarificationRequest)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (originalClarificationRequest == null)
                throw new ArgumentNullException("originalClarificationRequest");
            if (updatedClarificationRequest == null)
                throw new ArgumentNullException("updatedClarificationRequest");

            foreach (var question in updatedClarificationRequest.Questions)
            {
                originalClarificationRequest.Questions.Add(question);
            }
            uow.SaveChanges();
        }

        public void ProcessResponse(IClarificationRequest clarificationRequest)
        {
            if (clarificationRequest == null)
                throw new ArgumentNullException("clarificationRequest");
            
            var answeredQuestions = clarificationRequest.Questions.Where(q => q.Status == QuestionStatus.Answered).ToArray();
            if (answeredQuestions.Length == 0)
                throw new ArgumentException("Clarification Request must have at least one answered question");
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curClarificationRequestDO = uow.ClarificationRequestRepository.GetByKey(clarificationRequest.Id);
                if (curClarificationRequestDO == null)
                    throw new EntityNotFoundException<ClarificationRequestDO>();
                var curBookingRequestDO = uow.BookingRequestRepository.GetByKey(clarificationRequest.BookingRequestId);
                if (curBookingRequestDO == null)
                    throw new EntityNotFoundException<BookingRequestDO>();
                foreach (var answeredQuestion in answeredQuestions)
                {
                    var questionDO = curClarificationRequestDO.Questions.FirstOrDefault(q => q.Id == answeredQuestion.Id);
                    if (questionDO == null)
                        throw new EntityNotFoundException<QuestionDO>();
                    questionDO.Response = answeredQuestion.Response;
                    questionDO.Status = QuestionStatus.Answered;
                }
                curClarificationRequestDO.ClarificationStatus = ClarificationStatus.Resolved;
                curBookingRequestDO.BRState = BRState.Pending;
                uow.SaveChanges();
            }
        }
    }
}