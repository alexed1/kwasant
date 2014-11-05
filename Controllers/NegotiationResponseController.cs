using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.States.Templates;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        private const bool EnforceUserInAttendees = true;
        private IAttendee _attendee;
        private IQuestion _question;
        private INegotiation _negotiation;

        //The main NegotiationResponse view displays Question and Answer data to an attendee
        [KwasantAuthorize(Roles = "Customer")]
        public ActionResult View(int negotiationID)
        {
            AuthenticateUser(negotiationID);
            
            var user = new User();
            var userID = this.GetUserId();
          
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userID);
                
                _question = new Question();
                _negotiation = new Negotiation();


                var curNegotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (curNegotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                //get all of the Answers responded to by this user
                var userAnswerIDs = _negotiation.GetAnswersByUser(curNegotiationDO,userDO,uow);

                var originatingUser = curNegotiationDO.BookingRequest.User.FirstName;
                if (!String.IsNullOrEmpty(curNegotiationDO.BookingRequest.User.LastName))
                    originatingUser += " " + curNegotiationDO.BookingRequest.User.LastName;


                var model = new NegotiationResponseVM
                {
                    Id = curNegotiationDO.Id,
                    Name = curNegotiationDO.Name,
                    BookingRequestID = curNegotiationDO.BookingRequestID,

                    CommunicationMode = user.GetMode(userDO),
                    OriginatingUser = originatingUser,

                    Attendees = curNegotiationDO.Attendees.Select(a => a.Name).ToList(),

                    //Building the List of NegotiationQuestionVM's
                    //Starting with all of the Questions in the Negotiation...
                    Questions = curNegotiationDO.Questions.Select(q =>
                    {
                        //select the Answer that is in our list of the answers to which the  user has responded
                        // var selectedAnswer = q.Answers.FirstOrDefault(a => userAnswerIDs.Contains(a.Id));
                        var selectedAnswer = _attendee.GetSelectedAnswer(q, userAnswerIDs);

                        
                        //build a list of NegotiationAnswerVMs
                        var answers = q.Answers.Select(a =>
                            (NegotiationAnswerVM) new NegotiationResponseAnswerVM
                            {
                                Id = a.Id,

                                //indicates which one, if any, the user has previously provided as a response 
                                Selected = a == selectedAnswer,
                                EventID = a.EventID,
                                UserAnswer = a.UserID == userID,
                                SuggestedBy = a.UserDO == null ? String.Empty : a.UserDO.UserName,

                                EventStartDate = a.Event == null ? (DateTimeOffset?) null : a.Event.StartDate,
                                EventEndDate = a.Event == null ? (DateTimeOffset?) null : a.Event.EndDate,

                                Text = a.Text,
                            }).OrderBy(a => a.EventStartDate).ThenBy(a => a.EventEndDate).ToList();

                        //We select the answer that the user previously selected
                        //If they don't have a previous selection, then we select the first answer by default. this encourages attendees to agree to what has been proposed by the booker.
                        //this Selected concept is presentation-specific, so it belongs here in the VM code
                        if (!answers.Any(a => a.Selected))
                            answers.First().Selected = true;

                        //Pack the list of NegotiationAnswerVMs into a NegotiationQuestionVM
                        return (NegotiationQuestionVM) new NegotiationResponseQuestionVM
                        {
                            Type = q.AnswerType,
                            Id = q.Id,
                            Text = q.Text,
                            CalendarID = q.CalendarID,

                            Answers = answers
                        };
                    }).ToList()
                };

                return View(model);
            }
        }

        [KwasantAuthorize(Roles = "Customer")]
        [HttpPost]
        public ActionResult ProcessResponse(NegotiationVM value)
        {
            if (!value.Id.HasValue)
                throw new HttpException(400, "Invalid parameter");

            AuthenticateUser(value.Id.Value);

            var userID = this.GetUserId();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(value.Id);
                if (negotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                //Here we add/update questions based on our proposed negotiation
                foreach (var question in value.Questions)
                {
                    if (question.Id == 0)
                        throw new HttpException(400, "Invalid parameter: Id of question cannot be 0.");
                    
                    var questionDO = uow.QuestionRepository.GetByKey(question.Id);

                    var currentSelectedAnswers = new List<AnswerDO>();
                    //Previous answers are read-only, we only allow updating of new answers
                    foreach (var answer in question.Answers)
                    {
                        AnswerDO answerDO;
                        if (answer.Id == 0)
                        {
                            if (!answer.Selected)
                                continue;

                            answerDO = new AnswerDO();
                            uow.AnswerRepository.Add(answerDO);

                            answerDO.Question = questionDO;
                            if (answerDO.AnswerStatus == 0)
                                answerDO.AnswerStatus = AnswerState.Proposed;

                            answerDO.Text = answer.Text;
                            answerDO.EventID = answer.EventID;
                            answerDO.UserID = userID;
                        } else
                        {
                            answerDO = uow.AnswerRepository.GetByKey(answer.Id);
                        }
                        if (answer.Selected)
                            currentSelectedAnswers.Add(answerDO);
                    }

                    var previousAnswers = uow.QuestionResponseRepository.GetQuery()
                        .Where(qr =>
                            qr.Answer.QuestionID == question.Id &&
                            qr.UserID == userID).ToList();

                    var previousAnswerIds = previousAnswers.Select(a => a.AnswerID).ToList();

                    var currentSelectedAnswerIDs = question.Answers.Where(a => a.Selected).Select(a => a.Id).ToList();

                    //First, remove old answers
                    foreach (var previousAnswer in previousAnswers.Where(previousAnswer => !previousAnswer.AnswerID.HasValue || !currentSelectedAnswerIDs.Contains(previousAnswer.AnswerID.Value)))
                    {
                        uow.QuestionResponseRepository.Remove(previousAnswer);
                    }

                    //Add new answers
                    foreach (var currentSelectedAnswer in currentSelectedAnswers.Where(a => !previousAnswerIds.Contains(a.Id)))
                    {
                        var newAnswer = new QuestionResponseDO
                        {
                            Answer = currentSelectedAnswer,
                            UserID = userID
                        };
                        uow.QuestionResponseRepository.Add(newAnswer);
                    }
                }

                if (negotiationDO.NegotiationState == NegotiationState.Resolved)
                {
                    AlertManager.PostResolutionNegotiationResponseReceived(negotiationDO.Id);
                }

                uow.SaveChanges();

                return View();
            }
        }

        public void AuthenticateUser(int negotiationID)
        {
            //If this is a regular customer, verify that they're an attendee
            var userID = this.GetUserId();
            var user = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!user.VerifyMinimumRole("Booker", userID, uow))
                    ConfirmUserInAttendees(uow, negotiationID);
            }
        }


        //verify that the person trying to view this negotiation is one of the attendees.
        public void ConfirmUserInAttendees(IUnitOfWork uow, int negotiationID)
        {
            if (!EnforceUserInAttendees)
                return;

            var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
            if (negotiationDO == null)
                throw new HttpException(404, "Negotiation not found.");

            var attendees = negotiationDO.Attendees;
            var currentUserID = this.GetUserId();

            var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == currentUserID);
            if (existingUserDO == null)
                throw new HttpException(404, "We don't have a User record for you. ");

            var currentUserEmail = existingUserDO.EmailAddress.Address.ToLower();

            foreach (var attendee in attendees)
                if (attendee.EmailAddress.Address.ToLower() == currentUserEmail)
                    return;

            throw new HttpException(403, "You're not authorized to view information about this Negotiation");
        }

	}
}