using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using AutoMapper;
using Data.Infrastructure;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private IMappingEngine _mappingEngine;

        public NegotiationController()
        {
            _mappingEngine = Mapper.Engine; // should be injected
        }

        public ActionResult Edit(int negotiationID, int bookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");

                var model = _mappingEngine.Map<EditNegotiationVM>(negotiationDO);

                // NOTE: code below is to add BookerID in BookingRequest if Another booker will loging
                BookingRequestDO bookingRequestDO = null;
                bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);
                if (bookingRequestDO.BookerID != this.GetUserId())
                {
                    bookingRequestDO.BookerID = this.GetUserId();
                    bookingRequestDO.User = bookingRequestDO.User;
                    uow.SaveChanges();
                    AlertManager.BookingRequestOwnershipChange(bookingRequestID, this.GetUserId());
                }

                return View(model);
            }
        }


        public ActionResult Create(int bookingRequestID)
        {
           
            return View("~/Views/Negotiation/Edit.cshtml", new EditNegotiationVM
            {
                Name = "Negotiation 1",
                BookingRequestID = bookingRequestID,
                State = NegotiationState.InProcess,
            });
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(EditNegotiationVM value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                NegotiationDO negotiationDO;
                if (value.Id == 0)
                {
                    negotiationDO = new NegotiationDO();
                    uow.NegotiationsRepository.Add(negotiationDO);
                }
                else
                    negotiationDO = uow.NegotiationsRepository.GetByKey(value.Id);

                // these collections will be used further
                var questions = negotiationDO.Questions.ToArray();
                var answers = negotiationDO.Questions.SelectMany(q => q.Answers).ToArray();

                negotiationDO = _mappingEngine.Map(value, negotiationDO);

                // NOTE: code below is to remove orphan objects. EF doesn't support their automatic removing.
                #region Remove Orphan Objects
                // questions to remove
                var questionsToRemove = questions
                    .Select(q => q.Id)
                    .Except(negotiationDO.Questions.Select(q => q.Id))
                    .SelectMany(id => questions.Where(q => q.Id == id)).ToArray();
                // answers to remove. excluding answers of removed questions as they are being removed automatically
                var answersToRemove = answers
                    .Select(a => a.Id)
                    .Except(negotiationDO.Questions.SelectMany(q => q.Answers).Select(a => a.Id))
                    .SelectMany(id => answers.Where(a => a.Id == id))
                    .Where(a => !questionsToRemove.Any(q => q.Id == a.QuestionID))
                    .ToArray();

                foreach (var questionDO in questionsToRemove)
                {
                    uow.QuestionsRepository.Remove(questionDO);
                }

                foreach (var answerDO in answersToRemove)
                {
                    uow.AnswersRepository.Remove(answerDO);
                }
                #endregion

                // Attendees managing
                var attendee = new Attendee();
                attendee.ManageNegotiationAttendeeList(uow, negotiationDO, value.Attendees);

                uow.SaveChanges();
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}