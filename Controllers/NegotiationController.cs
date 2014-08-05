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
        private static int BookingRequestId { get; set; }
        private Answer _answer;
        #region "Negotiation"

        public NegotiationController()
        {
            _answer = new Answer();
        }
        
        public ActionResult Edit(int bookingRequestID)
        {
            BookingRequestId = bookingRequestID;
            return View();
        }

        public ActionResult Create(string negotiation)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            NegotiationViewModel viewModel = json_serializer.Deserialize<NegotiationViewModel>(negotiation);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestId);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                NegotiationDO negotiationDO = new NegotiationDO
                {
                    Name = viewModel.Name,
                    RequestId = BookingRequestId,
                    NegotiationStateID = NegotiationState.InProcess,
                    Email = emailDO
                };
                uow.NegotiationsRepository.Add(negotiationDO);
                uow.SaveChanges();
                var result = new { Success = "True", NegotiationId = negotiationDO.Id };
                return Json(result, JsonRequestBehavior.AllowGet);
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
                    RequestId = BookingRequestId,
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
                            AnswerStatusID = ansl.AnswerStatusID,
                            //Status = ansl.AnswerStatus,
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
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestId);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                NegotiationDO negotiationsDO = uow.NegotiationsRepository.FindOne(n => n.RequestId == BookingRequestId && n.NegotiationStateID != NegotiationState.Resolved);

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
                            answerDO.AnswerStatusID = answers.AnswerStatusID;
                            answerDO.ObjectsType = answers.Text;
                            uow.SaveChanges();
                        }
                    }
                }
                var result = new { Success = "True", BookingRequestID = emailDO.Id, NegotiationId = negotiationDO.Id };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EventWindows(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestRepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(BookingRequestId);
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
