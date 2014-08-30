using System;
using System.Collections.Generic;
using System.Linq;
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
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");
                
                var model = new NegotiationVM
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
                            Text = q.Text,
                            NegotiationId = negotiationDO.Id,
                            CalendarEvents = q.Calendar == null ? new List<QuestionCalendarEventVM>() : q.Calendar.Events.Select(e => new QuestionCalendarEventVM
                            {
                                StartDate = e.StartDate,
                                EndDate = e.EndDate
                            }).ToList(),
                            
                            CalendarID = q.CalendarID,
                            Answers = q.Answers.Select(a =>
                                new NegotiationAnswerVM
                                {
                                    Id = a.Id,
                                    QuestionId = q.Id,
                                    Text = a.Text
                                }).ToList()
                        }
                        ).ToList()
                };

                return View(model);
            }
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

                        answerDO.Question = questionDO;
                        if (answerDO.AnswerStatus == 0)
                            answerDO.AnswerStatus = AnswerState.Proposed;
                        
                        answerDO.Text = answer.Text;
                    }
                }

                var calendarIDs = value.Questions.Select(a => a.CalendarID).Where(c => c != null).ToList();
                var calendars = uow.CalendarRepository.GetQuery().Where(c => c.NegotiationID == null && calendarIDs.Contains(c.Id));
                foreach (var calendar in calendars)
                {
                    calendar.Negotiation = negotiationDO;
                }

                uow.SaveChanges();

                using (var subUoW = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var communicationManager = new CommunicationManager();
                    communicationManager.DispatchNegotiationRequests(subUoW, negotiationDO.Id);
                    subUoW.SaveChanges();                    
                }

                return Json(negotiationDO.Id, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(int negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
                foreach (var calendar in negotiationDO.Calendars)
                    calendar.NegotiationID = null;

                uow.NegotiationsRepository.Remove(negotiationDO);
                uow.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}