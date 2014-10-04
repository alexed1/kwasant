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
                bookingRequestDO.BookerID = currBooker;
                bookingRequestDO.LastUpdated = DateTimeOffset.Now;
                uow.SaveChanges();
                AlertManager.BookingRequestCheckedOut(bookingRequestDO.Id, currBooker);

                //Get the most recent conversation for this Booking Request
                var curEmail = uow.EmailRepository.GetAll().Where(e => e.Id == id || e.ConversationId == id).OrderByDescending(e => e.DateReceived).First();
                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                var attachmentInfo = String.Join("<br />",
                            curEmail.Attachments.Select(
                                attachment =>
                                "<a href='" + String.Format(fileViewURLStr, attachment.Id) + "' target='" +
                                attachment.OriginalName + "'>" + attachment.OriginalName + "</a>"));

                string booker = "none";
                string bookerId = uow.BookingRequestRepository.GetByKey(id).BookerID;
                if (bookerId != null)
                {
                    var curbooker = uow.UserRepository.GetByKey(bookerId);
                    if (curbooker.EmailAddress != null)
                        booker = curbooker.EmailAddress.Address;
                    else
                        booker = curbooker.FirstName;
                }

                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    ConversationMembers = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestDO.Id).Select(e => e.Id).ToList(),
                    BookingRequestId = bookingRequestDO.Id,
                    CurEmailData = new EmailDO
                    {
                        Attachments = curEmail.Attachments,
                        From = curEmail.From,
                        Recipients = curEmail.Recipients,
                        HTMLText = curEmail.HTMLText,
                        Id = curEmail.Id,
                        FromID = curEmail.FromID,
                        DateCreated = curEmail.DateCreated,
                        Subject = curEmail.Subject
                    },
                    EmailTo = String.Join(", ", curEmail.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", curEmail.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", curEmail.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    Booker = booker
                };
                TempData["requestInfo"] = bookingInfo;
            //Redirect to Calendar control to open Booking Agent UI. It takes email id as parameter to which email message will be dispalyed in the left column of Booking Agent UI
                return RedirectToAction("Index", "Dashboard", new { id = id });
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

                    return new JsonResult() { Data = new { Message = "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be", UserID = bookingRequest.UserID }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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

        [HttpPost]
        public void ReleaseBooker(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.BookerID = null;
                bookingRequestDO.User = bookingRequestDO.User;
                uow.SaveChanges();
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


    }
}