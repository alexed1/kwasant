using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using KwasantWeb.ViewModels.JsonConverters;
using Segment;
using Segment.Model;
using StructureMap;
using System.Net.Mail;
using System;
using Data.Repositories;
using Data.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class BookingRequestController : Controller
    {
        private DataTablesPackager _datatables;
        private BookingRequest _br;
        private int recordcount;
        Booker _booker;
        
        public BookingRequestController()
        {
            _datatables = new DataTablesPackager();
            _br = new BookingRequest();
            _booker = new Booker();
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
                var jsonResult = Json(_datatables.Pack(_br.GetUnprocessed(uow)), JsonRequestBehavior.AllowGet);
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
            var currBooker = this.GetUserId();
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                if (bookingRequestDO == null)
                    return HttpNotFound();
                bookingRequestDO.State = BookingRequestState.Booking;
                bookingRequestDO.UserID = currBooker;
                bookingRequestDO.LastUpdated = DateTimeOffset.Now;
                uow.SaveChanges();
                AlertManager.BookingRequestCheckedOut(bookingRequestDO.Id, currBooker);
            }

            //Redirect to Calendar control to open Booking Agent UI. It takes email id as parameter to which email message will be dispalyed in the left column of Booking Agent UI
            return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index", id = id }));
        }

        [HttpGet]
        public ActionResult ProcessOwnerChange(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currBooker = this.GetUserId();
                string result = _booker.ChangeOwner(uow, bookingRequestId, currBooker);
                return Content(result);
            }
        }

        [HttpGet]
        public ActionResult MarkAsProcessed(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //call to VerifyOwnership 
                var currBooker = this.GetUserId();
                string verifyOwnership = _booker.IsBookerValid(uow, id, currBooker);
                if (verifyOwnership != "valid")
                    return Json(new KwasantPackagedMessage { Name = "DifferentOwner", Message = verifyOwnership }, JsonRequestBehavior.AllowGet);

                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                bookingRequestDO.State = BookingRequestState.Resolved;
                uow.SaveChanges();
                AlertManager.BookingRequestStateChange(bookingRequestDO.Id);

                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" }, JsonRequestBehavior.AllowGet);
            }
        }

         [HttpGet]
         public ActionResult Invalidate(int id)
         {
             using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
             {
                 //call to VerifyOwnership
                 var currBooker = this.GetUserId();
                 string verifyOwnership = _booker.IsBookerValid(uow, id, currBooker);
                 if (verifyOwnership != "valid")
                     return Json(new KwasantPackagedMessage { Name = "DifferentOwner", Message = verifyOwnership }, JsonRequestBehavior.AllowGet);

                 BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                 bookingRequestDO.State = BookingRequestState.Invalid;
                 uow.SaveChanges();
                 AlertManager.BookingRequestStateChange(bookingRequestDO.Id);
                 return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" }, JsonRequestBehavior.AllowGet);
             }
         }

        [HttpGet]
        public ActionResult GetBookingRequests(int? bookingRequestId, int draw, int start, int length)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string userId = _br.GetUserId(uow.BookingRequestRepository, bookingRequestId.Value);
                int recordcount = _br.GetBookingRequestsCount(uow.BookingRequestRepository, userId);
                var jsonResult = Json(new
                {
                    draw = draw,
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,
                    data = _datatables.Pack(_br.GetAllByUserId(uow.BookingRequestRepository, start, length, userId))
                }, JsonRequestBehavior.AllowGet);

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }


        [AllowAnonymous]
        public ActionResult Generate(string emailAddress,string meetingInfo)
        {
            string result = "";
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(emailAddress);
                    BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                    bookingRequest.DateReceived = DateTime.Now;
                    bookingRequest.PlainText = meetingInfo;
                    _br.Process(uow, bookingRequest);

                    uow.SaveChanges();

                    ObjectFactory.GetInstance<ISegmentIO>().Track(bookingRequest.User, "SiteActivity", "SubmitsViaTryItOut", new Dictionary<string, object> {{"BookingRequestID", bookingRequest.Id}});

                    return new JsonResult() { Data = new { Message = "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be", UserID = bookingRequest.UserID }, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }
            catch (Exception e)
            {
                return new JsonResult() { Data = new { Message = "Sorry! Something went wrong. Alpha software..." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            
        }

        // GET: /RelatedItems 
        [HttpGet]
        public ActionResult ShowRelatedItems(int bookingRequestId, int draw, int start, int length)
        {
            List<RelatedItemShowVM> obj = new List<RelatedItemShowVM>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            { 
                var jsonResult = Json(new
                {
                    draw = draw,
                    data = _datatables.Pack(BuildRelatedItemsJSON(uow, bookingRequestId, start, length)),
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,
                   
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public List<RelatedItemShowVM> BuildRelatedItemsJSON(IUnitOfWork uow, int bookingRequestId, int start, int length)
        {
            List<RelatedItemShowVM> bR_RelatedItems = _br
                .GetRelatedItems(uow, bookingRequestId)
                .Select(AutoMapper.Mapper.Map<RelatedItemShowVM>)
                .ToList();

            recordcount = bR_RelatedItems.Count;
            return bR_RelatedItems.OrderByDescending(x => x.Date).Skip(start).Take(length).ToList();
        }
    }
}