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
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    //[KwasantAuthorize(Roles = "Admin")]
    public class ClarificationRequestController : Controller
    {
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
                    clarificationRequest.Send(curClarificationRequestDO);
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


    }
}