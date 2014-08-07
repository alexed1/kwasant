using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using ViewModel.Models;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private Negotiation _negotiation;
        private Attendee _attendee;

        public NegotiationController()
        {
            _negotiation = new Negotiation();
            _attendee = new Attendee();
        }

        public ActionResult Edit(int negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");

                var model = new NegotiationViewModel
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,
                    State = negotiationDO.NegotiationState,
                    Questions = negotiationDO.Questions.Select(q =>
                        new QuestionViewModel
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            Status = q.QuestionStatus,
                            Text = q.Text,
                            NegotiationId = negotiationDO.Id,
                            Answers = q.Answers.Select(a =>
                            new AnswerViewModel
                            {
                                AnswerState = a.AnswerStatus,
                                Id = a.Id,
                                QuestionID = q.Id,
                                Text = a.Text,
                                ObjectsType = a.ObjectsType,
                                CalendarID = a.CalendarID
                            }).ToList()
                        }
                        ).ToList()
                };

                return View(model);
            }
        }

        //This is completely broken. Static means that it's stored for _all_ connections, including different users!!!
        //public ActionResult Create(NegotiationViewModel viewModel)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        BookingRequestDO emailDO = uow.BookingRequestRepository.FindOne(el => el.Id == BookingRequestID);
        //        UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

        //        //NEED TO CHECK HERE TO SEE IF THERE ALREADY IS ONE. SOMETHING LIKE:
        //        NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.BookingRequestID == BookingRequestID && n.NegotiationState != NegotiationState.Resolved);
        //        if (negotiationDO != null)
        //            throw new ApplicationException("tried to create a negotiation when one already existed");

        //        negotiationDO = new NegotiationDO
        //        {
        //            Name = viewModel.Name,
        //            BookingRequestID = BookingRequestID,
        //            NegotiationState = NegotiationState.InProcess,
        //            BookingRequest = emailDO
        //        };
        //        uow.NegotiationsRepository.Add(negotiationDO);
        //        uow.SaveChanges();
        //        var result = new { Success = "True", NegotiationId = negotiationDO.Id };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

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

        [HttpPost]
        public JsonResult ProcessSubmittedForm(NegotiationViewModel value)
        {
            var res = this;
            //NegotiationDO newNegotiationData = curVM.curNegotiation;
            //string attendeeList = curVM.attendeeList;

            //object result;
            //NegotiationDO updatedNegotiationDO = new NegotiationDO();
            //try
            //{
            //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //    {

            //        //the data passed up from the form should include a valid negotiationId.
            //        NegotiationDO curNegotiationDO =
            //            uow.NegotiationsRepository.FindOne(n => n.Id == newNegotiationData.Id);

            //        //Update Negotiation
            //        NegotiationDO existingNegotiationDO =
            //            uow.NegotiationsRepository.FindOne(n => n.Id == newNegotiationData.Id);
            //        updatedNegotiationDO = _negotiation.Update(newNegotiationData, existingNegotiationDO);

            //        //this takes the form data and processes it similarly to how its done in the Edit Event form
            //        //IMPORTANT: the code in Attendee.cs was refactored and needs testing.
            //        //_attendee.ManageNegotiationAttendeeList(uow, updatedNegotiationDO, attendeeList); //see


            //        //SEE https://maginot.atlassian.net/wiki/display/SH/CRUD+for+Questions%2C+Answers%2C+Negotiations


            //        //Process Negotiation
            //        _negotiation.Process(updatedNegotiationDO);
            //        //set result to a success message
            //        result =
            //            new
            //            {
            //                Success = "True",
            //                BookingRequestID = updatedNegotiationDO.BookingRequest.Id,
            //                NegotiationId = updatedNegotiationDO.Id
            //            };


            //    }
            //}
            //catch (Exception)
            //{
            //    //set result to an error message
            //    result =
            //        new
            //        {
            //            Success = "False",
            //            BookingRequestID = updatedNegotiationDO.BookingRequest.Id,
            //            NegotiationId = updatedNegotiationDO.Id
            //        };
            //}


            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }
}