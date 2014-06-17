using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [HandleError]
    [KwasantAuthorize(Roles = "Admin")]
    public class ClarificationRequestController : Controller
    {
        public ActionResult Edit(int bookingRequestId, int clarificationRequestId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId);
                var vm = Mapper.Map<ClarificationRequestDO, ClarificationRequestViewModel>(clarificationRequest);
                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult AddAnotherQuestion(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = GetOrCreateClarificationRequest(uow, viewModel.BookingRequestId, viewModel.Id);
                UpdateClarificationRequest(uow, viewModel, clarificationRequest);
                
                return RedirectToAction("Edit", new { bookingRequestId = clarificationRequest.BookingRequestId, clarificationRequestId = clarificationRequest.Id });
            }
        }

        [HttpPost]
        public ActionResult Send(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = GetOrCreateClarificationRequest(uow, viewModel.BookingRequestId, viewModel.Id);
                UpdateClarificationRequest(uow, viewModel, clarificationRequest);

                var crService = new ClarificationRequest(uow);
                crService.Send(clarificationRequest);

                return Json(true);
            }
        }


        private ClarificationRequestDO GetOrCreateClarificationRequest(IUnitOfWork uow, int bookingRequestId, int clarificationRequestId = 0)
        {
            ClarificationRequestDO clarificationRequest = null;
            if (clarificationRequestId > 0)
            {
                clarificationRequest = uow.ClarificationRequestRepository.GetByKey(clarificationRequestId);
            }
/*
            if (clarificationRequest == null)
            {
                clarificationRequest =
                    uow.ClarificationRequestRepository.FindOne(cr => cr.BookingRequestId == bookingRequestId);
            }
*/
            if (clarificationRequest == null)
            {
                var bookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequest == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, "Booking Request not found.");
                var crService = new ClarificationRequest(uow);
                clarificationRequest = crService.Create(bookingRequest);
            }
            return clarificationRequest;
        }

        private void UpdateClarificationRequest(IUnitOfWork uow, ClarificationRequestViewModel viewModel, ClarificationRequestDO clarificationRequest)
        {
            var recipients = viewModel.Recipients != null ? viewModel.Recipients.Split(',') : new string[0];
            foreach (var email in recipients)
            {
                if (!clarificationRequest.Recipients.Any(r => r.EmailAddress.Address == email))
                {
                    clarificationRequest.AddEmailRecipient(EmailParticipantType.TO, uow.EmailAddressRepository.GetOrCreateEmailAddress(email));
                }
            }

            var recipientsToDelete =
                clarificationRequest.Recipients.Where(
                    recipient => recipients.All(r => r != recipient.EmailAddress.Address));
            foreach (var recipientDo in recipientsToDelete)
            {
                uow.RecipientRepository.Remove(recipientDo);
                clarificationRequest.Recipients.Remove(recipientDo);
            }

            clarificationRequest.Questions.Add(new QuestionDO()
            {
                ClarificationRequest = clarificationRequest,
                ClarificationRequestId = clarificationRequest.Id,
                Status = QuestionStatus.Unanswered,
                Text = viewModel.Question
            });
            uow.SaveChanges();
        }

    }
}