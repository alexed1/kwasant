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
using ViewModel.Models;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private static int BookingRequestID { get; set; }
        private Negotiation _negotiation ;
        private Attendee _attendee;
        private static int BookingRequestId { get; set; }
        private Answer _answer;

        public NegotiationController()
        {
            _negotiation = new Negotiation();
            _attendee = new Attendee();
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
                BookingRequestDO emailDO = uow.BookingRequestRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                //NEED TO CHECK HERE TO SEE IF THERE ALREADY IS ONE. SOMETHING LIKE:
                NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.BookingRequestID == BookingRequestID && n.NegotiationStateID != NegotiationState.Resolved);
                if (negotiationDO != null)
                    throw new ApplicationException("tried to create a negotiation when one already existed");

                 negotiationDO = new NegotiationDO
                {
                    Name = viewModel.Name,
                    BookingRequestID = BookingRequestID,
                    NegotiationStateID = NegotiationState.InProcess,
                    BookingRequest = emailDO
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

                NegotiationViewModel NegotiationQuestions = uow.NegotiationsRepository.GetAll().Where(e => e.BookingRequestID == id && e.NegotiationStateID != NegotiationState.Resolved).Select(s => new NegotiationViewModel
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
                    NegotiationDO existingNegotiationDO =
                        uow.NegotiationsRepository.FindOne(n => n.Id == newNegotiationData.Id);
                    updatedNegotiationDO = _negotiation.Update(newNegotiationData, existingNegotiationDO);

                    //this takes the form data and processes it similarly to how its done in the Edit Event form
                    //IMPORTANT: the code in Attendee.cs was refactored and needs testing.
                    //_attendee.ManageNegotiationAttendeeList(uow, updatedNegotiationDO, attendeeList); //see


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

                    //if (questionDO != null)
                    //{
                    //    questionDO.QuestionStatus = question.Status;
                    //    questionDO.Text = question.Text;
                    //    questionDO.AnswerType = question.AnswerType;
                    //    uow.SaveChanges();

                    //    foreach (var answers in question.Answers)
                    //    {
                    //        AnswerDO answerDO = uow.AnswersRepository.FindOne(a => a.Id == answers.Id);
                    //        answerDO.AnswerStatusID = answers.AnswerStatusID;
                    //        answerDO.ObjectsType = answers.Text;
                    //        uow.SaveChanges();
                    //    }
                }
            }
            catch (Exception)
            {
                //set result to an error message
                result =
                    new
                    {
                        Success = "False",
                        BookingRequestID = updatedNegotiationDO.BookingRequest.Id,
                        NegotiationId = updatedNegotiationDO.Id
                    };
            }


            return Json(result, JsonRequestBehavior.AllowGet);
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
                    ActiveCalendarID = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).FirstOrDefault()
                });
            }
        }


    }
}
