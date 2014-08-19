using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using Utilities;
using StructureMap;
using BookingRequestState = Data.States.BookingRequestState;
using ClarificationRequestState = Data.States.ClarificationRequestState;

namespace KwasantCore.Services
{
    public class ClarificationRequest
    {
        private Email _email;
        public ClarificationRequest()
        {
            _email = new Email();
        }
        public ClarificationRequestDO Create(IUnitOfWork uow, IBookingRequest bookingRequest, NegotiationDO curNegotiation)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (bookingRequest == null)
                throw new ArgumentNullException("bookingRequest");
            var newClarificationRequestDo = new ClarificationRequestDO()
                                             {
                                                 DateCreated = DateTime.UtcNow,
                                                 DateReceived = DateTime.UtcNow,
                                                 NegotiationId = curNegotiation.Id,
                                                 Subject = "We need a little more information from you",
                                                 HTMLText = "*** This should be replaced with template body ***",
                                                 PlainText = "*** This should be replaced with template body ***",
                                             };
        
            String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];
            newClarificationRequestDo.From = Email.GenerateEmailAddress(uow, new MailAddress(senderMailAddress));
            newClarificationRequestDo.AddEmailRecipient(EmailParticipantType.To, bookingRequest.User.EmailAddress);
            return newClarificationRequestDo;
        }

 
        public void Send(ClarificationRequestDO curCR)
        {
            if (curCR == null)
                throw new ArgumentNullException("Clarification Request that was passed to Send was null");
            //do proper validation here
            var responseUrlFormat = "foo";  //replace with appropriate formatting. the previous code could only run in the controller: string.Concat(Url.Action("", RouteConfig.ShowNegotiationResponseUrl, new { }, this.Request.Url.Scheme), "?{0}");
            var responseUrl =  GenerateResponseURL(curCR, responseUrlFormat);
            _email.SendTemplate("clarification_request_v1", curCR, new Dictionary<string, string>
                                                                        {
                                                                            { "RESP_URL", responseUrl }
                                                                        });
        }


        //public ActionResult Send(int bookingRequestId, int clarificationRequestId = 0, int negotiationId = 0)
        //{
           

              
        //            cr.Send(uow, curClarificationRequestDO, responseUrl);
        //            uow.SaveChanges();
        //            return Json(new { success = true });
        //        }
        //        catch (EntityNotFoundException ex)
        //        {
        //            return HttpNotFound(ex.Message);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { success = false, error = ex.Message });
        //        }
        //    }
        //}


        public string GenerateResponseURL(IClarificationRequest clarificationRequestDO, string responseUrlFormat)
        {
            if (clarificationRequestDO == null)
                throw new ArgumentNullException("clarificationRequestDO");
            var encryptedParams = WebUtility.UrlEncode(Encryption.EncryptParams(new { id = clarificationRequestDO.Id }));
            return string.Format(responseUrlFormat, encryptedParams);
        }

        public IClarificationRequest GetOrCreateClarificationRequest(IUnitOfWork uow, int bookingRequestId, int clarificationRequestId = 0, int NegotiationId=0)
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
                NegotiationDO negotiation = uow.NegotiationsRepository.GetByKey(NegotiationId);
                if (bookingRequest == null)
                    throw new EntityNotFoundException<IBookingRequest>();
                clarificationRequest = Create(uow, bookingRequest, negotiation);
            }
            return clarificationRequest;
        }

        //public void UpdateClarificationRequest(IUnitOfWork uow, IClarificationRequest originalClarificationRequest, IClarificationRequest updatedClarificationRequest)
        //{
        //    if (uow == null)
        //        throw new ArgumentNullException("uow");
        //    if (originalClarificationRequest == null)
        //        throw new ArgumentNullException("originalClarificationRequest");
        //    if (updatedClarificationRequest == null)
        //        throw new ArgumentNullException("updatedClarificationRequest");

        //    //foreach (var question in updatedClarificationRequest.Questions)
        //    //{
        //    //    originalClarificationRequest.Questions.Add(question);
        //    //}
        //    uow.SaveChanges();
        //}

        public void UpdateClarificationRequest(IUnitOfWork uow, IClarificationRequest originalClarificationRequest)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (originalClarificationRequest == null)
                throw new ArgumentNullException("originalClarificationRequest");
            
            uow.SaveChanges();
        }

        public void ProcessResponse(IClarificationRequest clarificationRequest)
        {


            //PROCESS Negotiation response
             //see https://maginot.atlassian.net/wiki/display/SH/Processing+Negotiation+Responses
            //Change the state of the ClarificationRequest to RepliedTo
            //Check to see if this response is the final response that this negotiation has been "waiting for". Do this by querying for all clarification requests that are associated with this clarification request's negotiation, and checking their state. If all states are now set to RepliedTo, the BookingRequest state, which should be AwaitingClient, should be changed to NeedsProcessing, so this BR goes back to a booker. 
            //Finally, render a simple confirmation view for the customer that thanks them and says that we'll notify them we've got the event resolved. 



            // the code below is obsolete and assumes a direct link with bookingrequest, which no longer is the case

            //if (clarificationRequest == null)
            //    throw new ArgumentNullException("clarificationRequest");
            
            //var answeredQuestions = clarificationRequest.Questions.Where(q => q.QuestionStatusID == QuestionStatus.Answered).ToArray();
            //if (answeredQuestions.Length == 0)
            //    throw new ArgumentException("Clarification Request must have at least one answered question");
            
            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    var curClarificationRequestDO = uow.ClarificationRequestRepository.GetByKey(clarificationRequest.Id);
            //    if (curClarificationRequestDO == null)
            //        throw new EntityNotFoundException<ClarificationRequestDO>();
            //    var curBookingRequestDO = uow.BookingRequestRepository.GetByKey(clarificationRequest.BookingRequestId);
            //    if (curBookingRequestDO == null)
            //        throw new EntityNotFoundException<BookingRequestDO>();
            //    foreach (var answeredQuestion in answeredQuestions)
            //    {
            //        var questionDO = curClarificationRequestDO.Questions.FirstOrDefault(q => q.Id == answeredQuestion.Id);
            //        if (questionDO == null)
            //            throw new EntityNotFoundException<QuestionDO>();
            //        questionDO.Response = answeredQuestion.Response;
            //        questionDO.QuestionStatusID = QuestionStatus.Answered;
            //    }
            //    curClarificationRequestDO.ClarificationRequestStateID = ClarificationRequestState.Resolved;
            //    curBookingRequestDO.BookingRequestStateID = BookingRequestState.Pending;
            //    uow.SaveChanges();
            }
        }
    }
