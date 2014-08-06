using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
//using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.States;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Exceptions;
//using KwasantCore.Managers.IdentityManager;
using KwasantCore.Services;
using KwasantWeb.App_Start;
using KwasantWeb.Filters;
using KwasantWeb.ViewModels;
using StructureMap;
using System.Web.Script.Serialization;
using Data.Repositories;
using ViewModel.Models;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private static int BookingRequestID { get; set; }
        private Negotiation _negotiation ;
        private Attendee _attendee;

        public NegotiationController()
        {
            _negotiation = new Negotiation();
            _attendee = new Attendee();
        }

        public ActionResult Edit(int bookingRequestID)
        {
            BookingRequestID = bookingRequestID;
            return View();
        }

        public ActionResult Create(string negotiation)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            NegotiationViewModel viewModel = json_serializer.Deserialize<NegotiationViewModel>(negotiation);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO emailDO = uow.BookingRequestRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                //NEED TO CHECK HERE TO SEE IF THERE ALREADY IS ONE. SOMETHING LIKE:
                NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.BookingRequestID == BookingRequestID && n.NegotiationState != NegotiationState.Resolved);
                if (negotiationDO != null)
                    throw new ApplicationException("tried to create a negotiation when one already existed");

                 negotiationDO = new NegotiationDO
                {
                    Name = viewModel.Name,
                    BookingRequestID = BookingRequestID,
                    NegotiationState = NegotiationState.InProcess,
                    BookingRequest = emailDO
                };
                uow.NegotiationsRepository.Add(negotiationDO);
                uow.SaveChanges();
                var result = new { Success = "True", NegotiationId = negotiationDO.Id };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteQuestion(int questionId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == questionId);
                if (calendarDO != null)
                    uow.CalendarRepository.Remove(calendarDO);

                uow.QuestionsRepository.Remove(uow.QuestionsRepository.FindOne(q => q.Id == questionId));
                uow.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ShowNegotiation(long id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //NegotiationViewModel NegotiationQuestions = new Negotiations().getNegotiation(uow, id);

                NegotiationViewModel NegotiationQuestions = uow.NegotiationsRepository.GetAll().Where(e => e.BookingRequestID == id && e.NegotiationState != NegotiationState.Resolved).Select(s => new NegotiationViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    RequestId = BookingRequestID,
                    State = s.NegotiationState,

                    Questions = uow.QuestionsRepository.GetAll().Where(que => que.NegotiationId == s.Id).Select(quel => new QuestionViewModel
                    {
                        Id = quel.Id,
                        Text = quel.Text,
                        Status = quel.QuestionStatusTemplate,
                        NegotiationId = quel.NegotiationId,
                        AnswerType = quel.AnswerType,
                        Answers = uow.AnswersRepository.GetAll().Where(ans => ans.QuestionID == quel.Id).Select(ansl => new AnswerViewModel
                        {
                            Id = ansl.Id,
                            QuestionID = ansl.QuestionID,
                            AnswerStatusId = ansl.AnswerStatus,
                            ObjectsType = ansl.ObjectsType,
                        }).ToList()
                    }).ToList()
                }).FirstOrDefault();

                //return View(NegotiationQuestions);
                return Json(NegotiationQuestions, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(EditNegotiationVM curVM )
        {
            NegotiationDO newNegotiationData = curVM.curNegotiation;
            string attendeeList = curVM.attendeeList;

            object result;
            NegotiationDO updatedNegotiationDO = new NegotiationDO();
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {

                    //the data passed up from the form should include a valid negotiationId.
                    NegotiationDO curNegotiationDO =
                        uow.NegotiationsRepository.FindOne(n => n.Id == newNegotiationData.Id);

                    //Update Negotiation
                    NegotiationDO existingNegotiationDO = uow.NegotiationsRepository.FindOne(n => n.Id == newNegotiationData.Id);
                    updatedNegotiationDO = _negotiation.Update(newNegotiationData, existingNegotiationDO);

                    //this takes the form data and processes it similarly to how its done in the Edit Event form
                    //IMPORTANT: the code in Attendee.cs was refactored and needs testing.
                    //_attendee.ManageNegotiationAttendeeList(uow, updatedNegotiationDO, attendeeList); //see

                    //negotiationDO.Name = newNegotiationData.Name;
                    //negotiationDO.NegotiationState = newNegotiationData.State;
                    //uow.SaveChanges();

                    //foreach (var question in newNegotiationData.Questions)
                    //{
                    //    QuestionDO questionDO = uow.QuestionsRepository.FindOne(q => q.Id == question.Id);

                    //    if (questionDO != null)
                    //    {
                    //        questionDO.QuestionStatus = question.Status;
                    //        questionDO.Text = question.Text;
                    //        questionDO.AnswerType = question.AnswerType;
                    //        uow.SaveChanges();

                    //        foreach (var answers in question.Answers)
                    //        {
                    //            AnswerDO answerDO = uow.AnswersRepository.FindOne(a => a.Id == answers.Id);
                    //            answerDO.AnswerStatusID = answers.AnswerStatusId;
                    //            answerDO.ObjectsType = answers.Text;
                    //            uow.SaveChanges();
                    //        }
                    //    }
                    //}

                    //Process Negotiation
                    _negotiation.Process(updatedNegotiationDO);
                    //set result to a success message
                    result = new { Success = "True", BookingRequestID = updatedNegotiationDO.BookingRequest.Id, NegotiationId = updatedNegotiationDO.Id };

                }
            }
            catch (Exception)
            {
                //set result to an error message
                result = new { Success = "False", BookingRequestID = updatedNegotiationDO.BookingRequest.Id, NegotiationId = updatedNegotiationDO.Id };
            }

            
             return Json(result, JsonRequestBehavior.AllowGet);
        }
       

        [HttpGet]
        public PartialViewResult AddQuestion(int questionID, int negotiationId = 0)
        {
            List<int> questionVal = new List<int>();
            questionVal.Add(questionID);

            if (negotiationId > 0)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    QuestionDO questionDO = new QuestionDO
                    {
                        Negotiation = uow.NegotiationsRepository.FindOne(q => q.Id == negotiationId),
                        QuestionStatus = QuestionState.Unanswered,
                        Text = "Question",
                    };

                    uow.QuestionsRepository.Add(questionDO);
                    uow.SaveChanges();
                    questionVal.Add(questionDO.Id);
                }
            }
            else
                questionVal.Add(0);

            return PartialView("_Question", questionVal);
        }

        [HttpGet]
        public PartialViewResult AddtextAnswer(int answerID, int questiontblID = 0)
        {
            List<int> ansVal = new List<int>();
            ansVal.Add(answerID);

            if (questiontblID > 0)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestID);
                    UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                    AnswerDO answerDO = new AnswerDO
                    {
                        QuestionID = questiontblID,
                        AnswerStatus = AnswerState.Proposed,
                        User = userDO,
                    };

                    uow.AnswersRepository.Add(answerDO);
                    uow.SaveChanges();
                    ansVal.Add(answerDO.Id);
                }
            }
            else
                ansVal.Add(0);

            return PartialView("_TextAnswer", ansVal);
        }

        public PartialViewResult AddTimeslotAnswer(int answerID)
        {
            return PartialView("_TimeslotAnswer", answerID);
        }

        public ActionResult EventWindows(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestRepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(BookingRequestID);
                if (bookingRequestDO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                //return View(bookingRequestDO);
                return View(new CalendarViewModel
                {
                    BookingRequestID = bookingRequestDO.Id,
                    LinkedCalendarIDs = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).ToList(),

                    //In the future, we won't need this - the 'main' calendar will be picked by the booker
                    ActiveCalendarID = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).FirstOrDefault()
                });
            }
        }


    }
}
