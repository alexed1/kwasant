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
                var cr = new ClarificationRequest();

                try
                {
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (EntityNotFoundException<IBookingRequest>)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Send(ClarificationRequestViewModel viewModel)
        {
            var submittedClarificationRequestDO = Mapper.Map<ClarificationRequestDO>(viewModel);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cr = new ClarificationRequest();
                try
                {
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, submittedClarificationRequestDO.BookingRequestId, submittedClarificationRequestDO.Id);
                    cr.UpdateClarificationRequest(uow, curClarificationRequestDO, submittedClarificationRequestDO);
                    var responseUrlFormat = string.Concat(Url.Action("", RouteConfig.ShowClarificationResponseUrl, new { }, this.Request.Url.Scheme), "?{0}");
                    var responseUrl = cr.GenerateResponseURL(curClarificationRequestDO, responseUrlFormat);
                    cr.Send(uow, curClarificationRequestDO, responseUrl); 
                    uow.SaveChanges();
                    return Json(new { success = true });
                }
                catch (EntityNotFoundException ex)
                {
                    return HttpNotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }

        [RequestParamsEncryptedFilter]
        public ActionResult ShowClarificationResponse(long id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curClarificationRequestDO = uow.ClarificationRequestRepository.GetByKey(id);
                if (curClarificationRequestDO == null)
                    return HttpNotFound("Clarification request not found.");
                if (curClarificationRequestDO.BookingRequest == null)
                    return HttpNotFound("Booking request not found.");
                if (curClarificationRequestDO.Questions.Count(q => q.Status == QuestionStatus.Unanswered) == 0)
                    return View("~/Views/ClarificationResponse/AllAnswered.cshtml");
                var curClarificationResponseViewModel = Mapper.Map<ClarificationRequestDO, ClarificationResponseViewModel>(curClarificationRequestDO);
                return View("~/Views/ClarificationResponse/New.cshtml", curClarificationResponseViewModel);
            }
        }
    }
}