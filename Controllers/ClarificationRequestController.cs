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
using KwasantCore.Exceptions;
using KwasantCore.Managers.IdentityManager;
using KwasantCore.Services;
using KwasantWeb.App_Start;
using KwasantWeb.Filters;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class ClarificationRequestController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Edit(int bookingRequestId, int clarificationRequestId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = new ClarificationRequest(uow);

                try
                {
                    var curClarificationRequestDO = clarificationRequest.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (BookingRequestNotFoundException)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }

/*
        [HttpPost]
        public ActionResult AddAnotherQuestion(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = new ClarificationRequest(uow);
                try
                {
                    var curClarificationRequestDO = clarificationRequest.GetOrCreateClarificationRequest(uow,viewModel.BookingRequestId, viewModel.Id);
                    clarificationRequest.UpdateClarificationRequest(uow, curClarificationRequestDO, Mapper.Map<ClarificationRequestDO>(viewModel));
                    return RedirectToAction("Edit",
                                            new
                                                {
                                                    bookingRequestId = curClarificationRequestDO.BookingRequestId,
                                                    clarificationRequestId = curClarificationRequestDO.Id
                                                });
                }
                catch (BookingRequestNotFoundException)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }
*/

        [KwasantAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Send(ClarificationRequestViewModel viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var clarificationRequest = new ClarificationRequest(uow);
                try
                {
                    var curClarificationRequestDO = clarificationRequest.GetOrCreateClarificationRequest(uow,viewModel.BookingRequestId, viewModel.Id);
                    clarificationRequest.UpdateClarificationRequest(uow, curClarificationRequestDO, Mapper.Map<ClarificationRequestDO>(viewModel));
                    var responseUrlFormat = string.Concat(Url.Action("", RouteConfig.ShowClarificationResponseUrl, new { }, this.Request.Url.Scheme), "?{0}");
                    var responseUrl = clarificationRequest.GenerateResponseURL(curClarificationRequestDO, responseUrlFormat);
                    clarificationRequest.Send(curClarificationRequestDO, responseUrl); 
                    return Json(new { success = true });
                }
                catch (BookingRequestNotFoundException)
                {
                    return HttpNotFound("Booking request not found.");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }

        [KwasantAuthorize(Roles = "Customer")]
        [RequestParamsEncryptedFilter]
        public ActionResult ShowClarificationResponse(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curClarificationRequestDO = uow.ClarificationRequestRepository.GetByKey(id);
                if (curClarificationRequestDO == null)
                    return HttpNotFound("Clarification request not found.");
                if (curClarificationRequestDO.BookingRequest == null)
                    return HttpNotFound("Booking request not found.");
                var curClarificationResponseViewModel = Mapper.Map<ClarificationRequestDO, ClarificationResponseViewModel>(curClarificationRequestDO);
                return View("~/Views/ClarificationResponse/New.cshtml", curClarificationResponseViewModel);
            }
        }
    }
}