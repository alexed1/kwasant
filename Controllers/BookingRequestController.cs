using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using KwasantWeb.ViewModels.JsonConverters;
using StructureMap;
using System.Net.Mail;
using System;
using Data.Repositories;
using Data.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using AutoMapper;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Booker")]
    public class BookingRequestController : Controller
    {
        // private DataTablesPackager _datatables;
        private BookingRequest _br;
        private int recordcount;
        Booker _booker;
        private JsonPackager _jsonPackager;
        private readonly IMappingEngine _mappingEngine;

        public BookingRequestController()
        {
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
            // _datatables = new DataTablesPackager();
            _br = new BookingRequest();
            _booker = new Booker();
            _jsonPackager = new JsonPackager();
        }

        // GET: /BookingRequest/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ShowUnprocessed()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // var jsonResult = Json(_datatables.Pack(_br.GetUnprocessed(uow)), JsonRequestBehavior.AllowGet);
                var unprocessedBRs = _br.GetUnprocessed(uow);
                var jsonResult = Json(_jsonPackager.Pack(unprocessedBRs));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var currBooker = this.GetUserId();
            try
            {
                _br.CheckOut(id.Value, currBooker);
                return RedirectToAction("Index", "Dashboard", new { id });
            }
            catch (EntityNotFoundException)
            {
                return HttpNotFound();
            }
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

        [HttpPost]
        public ActionResult MarkAsProcessed(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //call to VerifyOwnership 
                var currBooker = this.GetUserId();
                string verifyOwnership = _booker.IsBookerValid(uow, id, currBooker);
                if (verifyOwnership != "valid")
                    return Json(new KwasantPackagedMessage { Name = "DifferentOwner", Message = verifyOwnership });

                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                bookingRequestDO.State = BookingRequestState.Resolved;
                uow.SaveChanges();
                AlertManager.BookingRequestStateChange(bookingRequestDO.Id);

                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" });
            }
        }

        [HttpPost]
        public ActionResult Invalidate(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //call to VerifyOwnership
                var currBooker = this.GetUserId();
                string verifyOwnership = _booker.IsBookerValid(uow, id, currBooker);
                if (verifyOwnership != "valid")
                    return Json(new KwasantPackagedMessage { Name = "DifferentOwner", Message = verifyOwnership });

                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                bookingRequestDO.State = BookingRequestState.Invalid;
                uow.SaveChanges();
                AlertManager.BookingRequestStateChange(bookingRequestDO.Id);
                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" });
            }
        }

        [HttpPost]
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
                    data = _jsonPackager.Pack(_br.GetAllByUserId(uow.BookingRequestRepository, start, length, userId))
                });

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult Generate(string emailAddress, string meetingInfo)
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

                    ObjectFactory.GetInstance<ITracker>().Track(bookingRequest.User, "SiteActivity", "SubmitsViaTryItOut", new Dictionary<string, object> { { "BookingRequestID", bookingRequest.Id } });

                    return Json(new
                        {
                            Message = "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be",
                            UserID = bookingRequest.UserID
                        });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "Sorry! Something went wrong. Alpha software..." });
            }

        }

        // GET: /RelatedItems 
        [HttpPost]
        public ActionResult ShowRelatedItems(int bookingRequestId, int draw, int start, int length)
        {
            List<RelatedItemShowVM> obj = new List<RelatedItemShowVM>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(new
                {
                    draw = draw,
                    data = _jsonPackager.Pack(BuildRelatedItemsJSON(uow, bookingRequestId, start, length)),
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,

                });
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

        [HttpPost]
        public ActionResult ReleaseBooker(int bookingRequestId)
        {
            try
            {
                _br.ReleaseBooker(bookingRequestId);
                return Json(true);
            }
            catch (EntityNotFoundException)
            {
                return HttpNotFound();
            }
        }

        public ActionResult ShowBRSOwnedByBooker()
        {
            var curBooker = this.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<BookingRequestDO> bookingRequestDO = _br.GetCheckedOut(uow, curBooker);
                BookingRequestsVM curBookingRequestsVM = new BookingRequestsVM
                {
                    BookingRequests =
                        bookingRequestDO.Select(e => _mappingEngine.Map<BookingRequestDO, BookingRequestVM>(e)).ToList()
                };

                return View("ShowMyBRs", curBookingRequestsVM);
            }
        }

        public ActionResult ShowInProcessBRS()
        {
            var curBooker = this.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<BookingRequestDO> bookingRequestDO = _br.GetCheckedOut(uow, curBooker);
                BookingRequestsVM curBookingRequestsVM = new BookingRequestsVM
                {
                    BookingRequests =
                        bookingRequestDO.Select(e => _mappingEngine.Map<BookingRequestDO, BookingRequestVM>(e)).ToList()
                };

                return View("ShowInProcessBRs", curBookingRequestsVM);
            }
        }


        // GET: /Conversation Members
        [HttpGet]
        public ActionResult ShowConversation(int bookingRequestId, int? curEmailId)
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestConversationVM bookingRequestConversation = new BookingRequestConversationVM
                {
                    FromAddress = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestId).Select(e => e.From.Address).ToList(),
                    DateReceived = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestId).ToList().Select(e => e.DateReceived.ToString("MMM dd") + _br.getCountDaysAgo(e.DateReceived)).ToList(),
                    ConversationMembers = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestId).Select(e => e.Id).ToList(),
                    HTMLText = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestId).Select(e => e.HTMLText).ToList(),
                    CurEmailId = curEmailId
                };

                return View(bookingRequestConversation);
            }
        }

        // GET: /BookingRequest/
        public ActionResult ShowAllBookingRequests()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAllBookingRequests()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_jsonPackager.Pack(_br.GetAllBookingRequests(uow)));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

    }
}