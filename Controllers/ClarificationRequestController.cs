using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.Filters;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class ClarificationRequestController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Create(int bookingRequestId, int clarificationRequestId = 0, int negotiationId=0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cr = new ClarificationRequest();
                try
                {
                    //NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(br => br.RequestId == bookingRequestId);
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId, negotiationId);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (EntityNotFoundException<IBookingRequest>)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Edit(int bookingRequestId, int clarificationRequestId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cr = new ClarificationRequest();

                try
                {
                    NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(br => br.BookingRequestID == bookingRequestId);
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId, negotiationDO.Id);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (EntityNotFoundException<IBookingRequest>)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }


        [RequestParamsEncryptedFilter]
        public ActionResult ShowClarificationResponse(int clarficationRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var questions = uow.ClarificationRequestRepository.GetByKey(clarficationRequestID);
                return null;
            }
        }

        public ActionResult ProcessResponse(string answerArray)
        {
           
            return Content("Success");
        }
    }
}