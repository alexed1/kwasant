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
using System.Web.Script.Serialization;

namespace KwasantWeb.Controllers
{
    public class CommunicationManagerController : Controller
    {
        //
        // GET: /CommunicationManager/
        public ActionResult DispatchNegotiationEmail(int BookingRequestID, int NegotiationId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int clarificationRequestId = 0;
                ClarificationRequestDO clarificationRequestDO;
                NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.RequestId == BookingRequestID && n.Id == NegotiationId && n.State != NegotiationState.Resolved);
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.FindOne(br => br.Id == BookingRequestID);
                if (negotiationDO == null)
                {
                    clarificationRequestDO = uow.ClarificationRequestRepository.FindOne(c => c.NegotiationId == negotiationDO.Id && c.BookingRequestId == bookingRequestDO.Id);
                    clarificationRequestId = clarificationRequestDO.Id;
                }

                var result = new { Success = "True", ClarificationRequestId = clarificationRequestId };
                return Json(result, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Send", "ClarificationRequest", new { bookingRequestId = BookingRequestID, clarificationRequestId = clarificationRequestId, negotiationId = NegotiationId });
            }
        }
    }
}