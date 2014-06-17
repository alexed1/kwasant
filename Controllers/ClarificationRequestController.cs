using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Managers.IdentityManager;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class ClarificationRequestController : Controller
    {
        public ActionResult Edit(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequest = uow.BookingRequestRepository.FindOne(br => br.Id == bookingRequestId);
                if (bookingRequest == null)
                    return HttpNotFound("Booking Request not found.");
                var crService = new ClarificationRequest(uow);
                var clarificationRequest = crService.Create(bookingRequest);
                
                var vm = Mapper.Map<ClarificationRequestDO, ClarificationRequestViewModel>(clarificationRequest);
                return View(vm);
            }
        }

        private void UpdateClarificationRequest(IUnitOfWork uow, ClarificationRequestViewModel viewModel, ClarificationRequestDO clarificationRequest)
        {
            clarificationRequest.Recipients.Clear();
            foreach (var email in viewModel.Recipients.Split(','))
            {
                clarificationRequest.AddEmailRecipient(EmailParticipantType.TO, uow.EmailAddressRepository.GetOrCreateEmailAddress(email));
            }
            clarificationRequest.Questions.Add(new ClarificationQuestionDO()
            {
                ClarificationRequest = clarificationRequest,
                ClarificationRequestId = clarificationRequest.Id,
                Status = ClarificationQuestionStatus.Unanswered,
                Text = viewModel.Question
            });
            uow.SaveChanges();
        }

        [HttpPost]
        public ActionResult AddAnotherQuestion(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequest = uow.BookingRequestRepository.FindOne(br => br.Id == viewModel.BookingRequestId);
                if (bookingRequest == null)
                    return HttpNotFound("Booking Request not found.");

                var crService = new ClarificationRequest(uow);
                var clarificationRequest = uow.ClarificationRequestRepository.FindOne(cr => cr.BookingRequestId == viewModel.BookingRequestId) ?? crService.Create(bookingRequest);
                UpdateClarificationRequest(uow, viewModel, clarificationRequest);
                
                viewModel.Question = string.Empty;

                return View("Edit", viewModel);
            }
        }

        [HttpPost]
        public ActionResult Send(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequest = uow.BookingRequestRepository.FindOne(br => br.Id == viewModel.BookingRequestId);
                if (bookingRequest == null)
                    return HttpNotFound("Booking Request not found.");
                
                var crService = new ClarificationRequest(uow);
                var clarificationRequest = uow.ClarificationRequestRepository.FindOne(cr => cr.BookingRequestId == viewModel.BookingRequestId) ?? crService.Create(bookingRequest);
                UpdateClarificationRequest(uow, viewModel, clarificationRequest);
                
                crService.Send(clarificationRequest);
                
                return JavaScript(SimpleJsonSerializer.Serialize(true));
            }
        }
    }
}