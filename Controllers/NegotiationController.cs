using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
//using Data.Entities.Enumerations;
using Data.Constants;
using Data.Interfaces;
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


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private static int BookingRequestID { get; set; }

        #region "Negotiation"

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
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                NegotiationDO negotiationDO = new NegotiationDO
                {
                    Name = viewModel.Name,
                    RequestId = BookingRequestID,
                    NegotiationStateID = NegotiationState.InProcess,
                    Email = emailDO
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

                NegotiationViewModel NegotiationQuestions = uow.NegotiationsRepository.GetAll().Where(e => e.RequestId == id && e.NegotiationStateID != NegotiationState.Resolved).Select(s => new NegotiationViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    RequestId = BookingRequestID,
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
                            AnswerStatusId = ansl.AnswerStatusID,
                            ObjectsType = ansl.ObjectsType,
                        }).ToList()
                    }).ToList()
                }).FirstOrDefault();

                //return View(NegotiationQuestions);
                return Json(NegotiationQuestions, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(string negotiation)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            NegotiationViewModel viewModel = json_serializer.Deserialize<NegotiationViewModel>(negotiation);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                NegotiationDO negotiationsDO = uow.NegotiationsRepository.FindOne(n => n.RequestId == BookingRequestID && n.NegotiationStateID != NegotiationState.Resolved);

                //Update Negotiation
                NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.Id == viewModel.Id);
                negotiationDO.Name = viewModel.Name;
                negotiationDO.NegotiationState = viewModel.State;
                uow.SaveChanges();

                foreach (var question in viewModel.Questions)
                {
                    QuestionDO questionDO = uow.QuestionsRepository.FindOne(q => q.Id == question.Id);

                    if (questionDO != null)
                    {
                        questionDO.QuestionStatus = question.Status;
                        questionDO.Text = question.Text;
                        questionDO.AnswerType = question.AnswerType;
                        uow.SaveChanges();

                        foreach (var answers in question.Answers)
                        {
                            AnswerDO answerDO = uow.AnswersRepository.FindOne(a => a.Id == answers.Id);
                            answerDO.AnswerStatusID = answers.AnswerStatusId;
                            answerDO.ObjectsType = answers.Text;
                            uow.SaveChanges();
                        }
                    }
                }
                var result = new { Success = "True", BookingRequestID = emailDO.Id, NegotiationId = negotiationDO.Id };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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
                        QuestionStatusID = QuestionStatus.Unanswered,
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
                        AnswerStatusID = AnswerStatus.Proposed,
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
                    MainCalendarID = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).FirstOrDefault()
                });
            }
        }

        #endregion "Negotiation"

    }
}
