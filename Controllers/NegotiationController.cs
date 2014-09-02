using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class NegotiationController : Controller
    {        
        public ActionResult Edit(int negotiationID)
        {
            return View(GetNegotiationVM(negotiationID, a => a.EventStartDate, a => a.EventEndDate));
        }

        public ActionResult Review(int negotiationID)
        {
            return
                View(GetNegotiationVM(negotiationID, a => a.AnswerState == AnswerState.Selected ? 0 : 1,
                    a => 1 - a.VotedBy.Count, a => (!a.EventStartDate.HasValue ? 0 : a.EventStartDate.Value.Ticks)));
        }

        private static NegotiationVM GetNegotiationVM<T>(int negotiationID, Func<NegotiationAnswerVM, T> orderByFunc, 
            Func<NegotiationAnswerVM, T> thenByFunc = null,
            Func<NegotiationAnswerVM, T> thenByFuncTwo = null)
        {
            NegotiationVM model;

            if (thenByFunc == null)
                thenByFunc = orderByFunc;
            if (thenByFuncTwo == null)
                thenByFuncTwo = orderByFunc;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");
                
                model = new NegotiationVM
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,
                    Attendees = negotiationDO.Attendees.Select(a => a.Name).ToList(),
                    Questions = negotiationDO.Questions.Select(q =>
                        new NegotiationQuestionVM
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            CalendarID = q.CalendarID,
                            Text = q.Text,
                            Answers = q.Answers.Select(a =>
                                new NegotiationAnswerVM
                                {
                                    Id = a.Id,
                                    Text = a.Text,
                                    AnswerState = a.AnswerStatus,
                                    VotedBy = uow.QuestionResponseRepository.GetQuery().Where(qr => qr.AnswerID == a.Id).Select(qr => qr.User.FirstName + " " + qr.User.LastName).ToList(),

                                    EventID = a.EventID,
                                    EventStartDate = a.Event == null ? (DateTimeOffset?)null : a.Event.StartDate,
                                    EventEndDate = a.Event == null ? (DateTimeOffset?)null : a.Event.EndDate,
                                })
                                .OrderBy(orderByFunc).ThenBy(thenByFunc).ThenBy(thenByFuncTwo)
                                .ToList()
                        }
                        ).ToList()
                };
            }
            return model;
        }


        public ActionResult Create(int bookingRequestID)
        {
            return View("~/Views/Negotiation/Edit.cshtml", new NegotiationVM
            {
                Name = "Negotiation 1",
                BookingRequestID = bookingRequestID,
            });
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(NegotiationVM value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                NegotiationDO negotiationDO;
                if (value.Id == null)
                {
                    negotiationDO = new NegotiationDO
                    {
                        DateCreated = DateTime.Now
                    };
                    uow.NegotiationsRepository.Add(negotiationDO);
                }
                else
                    negotiationDO = uow.NegotiationsRepository.GetByKey(value.Id);

                negotiationDO.Name = value.Name;
                if (negotiationDO.NegotiationState == 0)
                    negotiationDO.NegotiationState = NegotiationState.AwaitingClient;

                negotiationDO.BookingRequestID = value.BookingRequestID;

                var attendee = new Attendee();
                attendee.ManageNegotiationAttendeeList(uow, negotiationDO, value.Attendees);

                var proposedQuestionIDs = value.Questions.Select(q => q.Id);
                //Delete the existing questions which no longer exist in our proposed negotiation
                var existingQuestions = negotiationDO.Questions.ToList();
                foreach (var existingQuestion in existingQuestions.Where(q => !proposedQuestionIDs.Contains(q.Id)))
                {
                    uow.QuestionsRepository.Remove(existingQuestion);
                }

                //Here we add/update questions based on our proposed negotiation
                foreach (var question in value.Questions)
                {
                    QuestionDO questionDO;
                    if (question.Id == 0)
                    {
                        questionDO = new QuestionDO();
                        uow.QuestionsRepository.Add(questionDO);
                    }
                    else
                        questionDO = uow.QuestionsRepository.GetByKey(question.Id);

                    questionDO.Negotiation = negotiationDO;
                    questionDO.AnswerType = question.AnswerType;
                    if (questionDO.QuestionStatus == 0)
                        questionDO.QuestionStatus = QuestionState.Unanswered;
                    
                    questionDO.Text = question.Text;
                    questionDO.CalendarID = question.CalendarID;

                    var proposedAnswerIDs = question.Answers.Select(a => a.Id);
                    //Delete the existing answers which no longer exist in our proposed negotiation
                    var existingAnswers = questionDO.Answers.ToList();
                    foreach (var existingAnswer in existingAnswers.Where(a => !proposedAnswerIDs.Contains(a.Id)))
                    {
                        uow.AnswerRepository.Remove(existingAnswer);
                    }

                    foreach (var answer in question.Answers)
                    {
                        AnswerDO answerDO;
                        if (answer.Id == 0)
                        {
                            answerDO = new AnswerDO();
                            uow.AnswerRepository.Add(answerDO);
                        }
                        else
                            answerDO = uow.AnswerRepository.GetByKey(answer.Id);

                        answerDO.EventID = answer.EventID;
                        answerDO.AnswerStatus = answer.AnswerState;
                        answerDO.Question = questionDO;
                        answerDO.Text = answer.Text;
                    }
                }

                uow.SaveChanges();

                using (var subUoW = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var communicationManager = ObjectFactory.GetInstance<CommunicationManager>();
                    communicationManager.DispatchNegotiationRequests(subUoW, negotiationDO.Id);
                    subUoW.SaveChanges();                    
                }

                return Json(negotiationDO.Id, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MarkResolved(int negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
                if (negotiationDO == null)
                    throw new HttpException(400, "Negotiation with id '" + negotiationID + "' not found.");

                negotiationDO.NegotiationState = NegotiationState.Resolved;
                uow.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}