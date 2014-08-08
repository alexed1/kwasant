using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using System.Web.Script.Serialization;
using Data.Repositories;
using Utilities;
using ViewModel.Models;
using AutoMapper;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private Negotiation _negotiation;
        private Attendee _attendee;
        private IMappingEngine _mappingEngine;

        public NegotiationController()
        {
            _negotiation = new Negotiation();
            _attendee = new Attendee();
            _mappingEngine = Mapper.Engine; // should be injected
        }

        public ActionResult Edit(int bookingRequestID)
        {
            return View(new EditNegotiationVM() { RequestId = bookingRequestID });
        }

        public ActionResult Create(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO emailDO = uow.BookingRequestRepository.FindOne(el => el.Id == bookingRequestId);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                //NEED TO CHECK HERE TO SEE IF THERE ALREADY IS ONE. SOMETHING LIKE:
                NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.BookingRequestID == bookingRequestId && n.NegotiationState != NegotiationState.Resolved);
                if (negotiationDO != null)
                    throw new ApplicationException("tried to create a negotiation when one already existed");

                negotiationDO = new NegotiationDO
                {
                    Name = "Negotiation",
                    BookingRequestID = bookingRequestId,
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

        public JsonResult DeleteAnswer(int answerId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == answerId);
                if (calendarDO != null)
                    uow.CalendarRepository.Remove(calendarDO);

                uow.AnswersRepository.Remove(uow.AnswersRepository.FindOne(q => q.Id == answerId));
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
                var curNegotiationDO = uow.NegotiationsRepository.GetAll().FirstOrDefault(e => e.BookingRequestID == id && e.NegotiationState != NegotiationState.Resolved);
                var curNegotiationViewModel = _mappingEngine.Map<EditNegotiationVM>(curNegotiationDO);
/*
                NegotiationViewModel NegotiationQuestions = uow.NegotiationsRepository.GetAll().Where(e => e.BookingRequestID == id && e.NegotiationState != NegotiationState.Resolved).Select(s => new NegotiationViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    RequestId = viewModel.RequestId,
                    State = s.NegotiationState,

                    Questions = uow.QuestionsRepository.GetAll().Where(que => que.NegotiationId == s.Id).Select(quel => new QuestionViewModel
                    {
                        Id = quel.Id,
                        Text = quel.Text,
                        Status = quel.QuestionStatus,
                        NegotiationId = quel.NegotiationId,
                        AnswerType = quel.AnswerType,
                        Answers = uow.AnswersRepository.GetAll().Where(ans => ans.QuestionID == quel.Id).Select(ansl => new AnswerViewModel
                        {
                            Id = ansl.Id,
                            QuestionID = ansl.QuestionID,
                            AnswerStatusID = ansl.AnswerStatus,
                            //Status = ansl.AnswerStatus,
                            ObjectsType = ansl.ObjectsType,
                        }).ToList()
                    }).ToList()
                }).FirstOrDefault();
*/

                //return View(NegotiationQuestions);
                return Json(curNegotiationViewModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(EditNegotiationVM curVM)
        {
            object result;
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {

                    //Update Negotiation
                    NegotiationDO existingNegotiationDO = uow.NegotiationsRepository.GetByKey(curVM.Id);
                    NegotiationDO updatedNegotiationDO = _mappingEngine.Map(curVM, existingNegotiationDO);
                    //updatedNegotiationDO = _negotiation.Update(newNegotiationData, existingNegotiationDO);

                    //this takes the form data and processes it similarly to how its done in the Edit Event form
                    //IMPORTANT: the code in Attendee.cs was refactored and needs testing.
                    _attendee.ManageNegotiationAttendeeList(uow, updatedNegotiationDO, curVM.AttendeeList); //see

                    uow.SaveChanges();
                    //SEE https://maginot.atlassian.net/wiki/display/SH/CRUD+for+Questions%2C+Answers%2C+Negotiations

                    //Process Negotiation
                    _negotiation.Process(updatedNegotiationDO);
                    //set result to a success message
                    result =
                        new
                        {
                            Success = "True",
                            BookingRequestID = updatedNegotiationDO.BookingRequest.Id,
                            NegotiationId = updatedNegotiationDO.Id
                        };


                }
            }
            catch (Exception)
            {
                //set result to an error message
                result =
                    new
                    {
                        Success = "False",
                        BookingRequestID = curVM.RequestId,
                        NegotiationId = curVM.Id
                    };
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
        public PartialViewResult AddTextAnswer(int bookingRequestId, int answerID, int questiontblID = 0)
        {
            List<int> ansVal = new List<int>();
            ansVal.Add(answerID);

            if (questiontblID > 0)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == bookingRequestId);
                    UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                    AnswerDO answerDO = new AnswerDO
                    {
                        QuestionID = questiontblID,
                        AnswerStatus = AnswerState.Unstarted,
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

        public ActionResult EventWindows(int bookingRequestId, int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestRepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(bookingRequestId);
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