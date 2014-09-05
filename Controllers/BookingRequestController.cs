using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
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
        
        public BookingRequestController()
        {
            _datatables = new DataTablesPackager();
            _br = new BookingRequest();
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
        public ActionResult MarkAsProcessed(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                bookingRequestDO.BookingRequestState = BookingRequestState.Processed;
                bookingRequestDO.User = bookingRequestDO.User;
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
                 BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                 bookingRequestDO.BookingRequestState = BookingRequestState.Invalid;
                 bookingRequestDO.User = bookingRequestDO.User;
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


        //create a BookingRequest
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
                    result = "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be";
                }
            }
            catch (Exception)
            {
                result = "Sorry! Something went wrong. Alpha software...";
            }
            return Content(result);
        }

        // GET: /RelatedItems 
        [HttpGet]
        public ActionResult ShowRelatedItems(int bookingRequestId, int draw, int start, int length)
        {
            List<BR_RelatedItems> obj = new List<BR_RelatedItems>();
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

        public List<BR_RelatedItems> BuildRelatedItemsJSON(IUnitOfWork uow, int bookingRequestId, int start, int length)
        {
            List<BR_RelatedItems> bR_RelatedItems = _br
                .GetRelatedItems(uow, bookingRequestId)
                .Select(AutoMapper.Mapper.Map<BR_RelatedItems>)
                .ToList();

            recordcount = bR_RelatedItems.Count();
            return bR_RelatedItems.OrderByDescending(x => x.Date).Skip(start).Take(length).ToList();
        }
    }
}