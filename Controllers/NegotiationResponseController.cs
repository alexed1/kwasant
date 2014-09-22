using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        private const bool EnforceUserInAttendees = true;

        [KwasantAuthorize(Roles = "Customer")]
        public ActionResult View(int negotiationID)
        {
            AuthenticateUser(negotiationID);
            
            var user = new User();
            var userID = this.GetUserId();
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userID);

                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                var answerIDs = negotiationDO.Questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
                var userAnswerIDs = uow.QuestionResponseRepository.GetQuery().Where(qr => answerIDs.Contains(qr.AnswerID) && qr.UserID == userID).Select(a => a.AnswerID).ToList();

                var originatingUser = negotiationDO.BookingRequest.User.FirstName;
                if (!String.IsNullOrEmpty(negotiationDO.BookingRequest.User.LastName))
                    originatingUser += " " + negotiationDO.BookingRequest.User.LastName;
                var model = new NegotiationResponseVM
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,

                    CommunicationMode = user.GetMode(userDO),
                    OriginatingUser = originatingUser,

                    Attendees = negotiationDO.Attendees.Select(a => a.Name).ToList(),
                    Questions = negotiationDO.Questions.Select(q =>
                    {
                        var selectedAnswer = q.Answers.FirstOrDefault(a => userAnswerIDs.Contains(a.Id));

                        var answers = q.Answers.Select(a =>
                                (NegotiationAnswerVM) new NegotiationResponseAnswerVM
                                {
                                    Id = a.Id,

                                Selected = a == selectedAnswer,
                                    EventID = a.EventID,
                                UserAnswer = a.UserID == userID,

                                    EventStartDate = a.Event == null ? (DateTimeOffset?)null : a.Event.StartDate,
                                    EventEndDate = a.Event == null ? (DateTimeOffset?)null : a.Event.EndDate,

                                    Text = a.Text,
                            }).OrderBy(a => a.EventStartDate).ThenBy(a => a.EventEndDate).ToList();

                        //We select the answer that the user previously selected
                        //If they don't have a previous selection, then we select the first answer by default.
                        if (!answers.Any(a => a.Selected))
                            answers.First().Selected = true;

                        return (NegotiationQuestionVM) new NegotiationResponseQuestionVM
                        {
                            AnswerType = q.AnswerType,
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
                if (value.Id == null)
                    throw new HttpException(404, "Negotiation not found");
                
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
                    foreach (var previousAnswer in previousAnswers.Where(previousAnswer => !currentSelectedAnswerIDs.Contains(previousAnswer.AnswerID)))
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