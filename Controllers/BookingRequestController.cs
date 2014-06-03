using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using Utilities;
using Utilities.Logging;

namespace KwasantWeb.Controllers
{
    public class BookingRequestController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestRepository curBookingRequestRepository;
        private DatatablesPackager API;

        public BookingRequestController(IUnitOfWork uow)
        {
            _uow = uow;
            curBookingRequestRepository = _uow.BookingRequestRepository;
            API = new DatatablesPackager();
        }

        // GET: /BookingRequest/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetUnprocessedRequest()
        {
            var jsonResult = Json(API.PackDatatableObject(curBookingRequestRepository.GetAll().Where(e => e.Status == EmailStatus.UNPROCESSED).OrderByDescending(e => e.Id).ToList()), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        // GET: /BookingRequest/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingRequestDO bookingRequestDO = curBookingRequestRepository.GetByKey(id);

            if (bookingRequestDO == null)
            {
                return HttpNotFound();
            }
            else
            {
                //Redirect to Calendar control to open Booking Agent UI. It takes email id as parameter to which email message will be dispalyed in the left column of Booking Agent UI
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Calendar", action = "Index", id = id }));
            }
            //return View(email);
        }

	}
}