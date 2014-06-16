using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using KwasantCore.Services;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace KwasantWeb.Controllers
{
    public class BookingRequestController : Controller
    {
        private DataTablesPackager _datatables;

        public BookingRequestController()
        {
            _datatables = new DataTablesPackager();
        }

        // GET: /BookingRequest/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShowUnprocessed()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_datatables.Pack(new BookingRequest().GetUnprocessed(uow.BookingRequestRepository)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingRequestDO bookingRequestDO = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);   
            }

            if (bookingRequestDO == null)
            {
                return HttpNotFound();
            }
            else
            {
                //Redirect to Calendar control to open Booking Agent UI. It takes email id as parameter to which email message will be dispalyed in the left column of Booking Agent UI
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Calendar", action = "Index", id = id }));
            }
        }


        [HttpGet]
        public ActionResult SetStatus(int? id, string targetStatus)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (id == null)
                {
                    return Json(new Error { Name = "Parameter Missing", Message = "Id is required" }, JsonRequestBehavior.AllowGet);
                }
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                if (bookingRequestDO == null)
                {
                    return Json(new Error { Name = "Invalid Request", Message = "Booking Request does not exists" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    (new BookingRequest()).SetStatus(uow, bookingRequestDO, targetStatus.ToLower());
                    switch (targetStatus.ToLower())
                    {
                        case "invalid":
                            return Json(new Error { Name = "Success", Message = "Status changed successfully" }, JsonRequestBehavior.AllowGet);
                        case "processed":
                            return RedirectToAction("Index", "BookingRequest");
                        default:
                            return Json(new Error { Name = "Failure", Message = "Invalid status change request" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult GetBookingRequests(int? id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_datatables.Pack((new BookingRequest()).GetBookingRequests(uow.BookingRequestRepository, id.Value)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
	}
}