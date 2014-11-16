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
using Data.Validations;
using FluentValidation;
using Utilities.Logging;

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
        public ActionResult ProcessBookerChange(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currBooker = this.GetUserId();
                string result = _booker.ChangeBooker(uow, bookingRequestId, currBooker);
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
                string verifyBooker = _booker.IsBookerValid(uow, id, currBooker);
                if (verifyBooker != "valid")
                    return Json(new KwasantPackagedMessage { Name = "DifferentBooker", Message = verifyBooker });

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
                    return Json(new KwasantPackagedMessage { Name = "DifferentBooker", Message = verifyOwnership });

                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(id);
                bookingRequestDO.State = BookingRequestState.Invalid;
                uow.SaveChanges();
                AlertManager.BookingRequestStateChange(bookingRequestDO.Id);
                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" });
            }
        }

        [HttpPost]
        public ActionResult ShowByUser(int? bookingRequestId, int draw, int start, int length)
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

        public ActionResult ShowManualCreationForm()
        {
            return View();
        }
        //Get all checkout BR's owned by the logged
        [HttpPost]
        public ActionResult ShowByBooker()
        {
            var curBooker = this.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //var jsonResult = Json(_datatables.Pack(_br.GetCheckOutBookingRequest(uow, curBooker)), JsonRequestBehavior.AllowGet);
                var bookerOwnedRequests = _br.GetCheckOutBookingRequest(uow, curBooker);
                var jsonResult = Json(_jsonPackager.Pack(bookerOwnedRequests));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public ActionResult ShowBRSOwnedByBooker()
        {
            return View("ShowMyBRs");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateViaBooker(string emailAddress, string meetingInfo, string subject)
        {
            try
            {
                var emailAddressDO = new EmailAddressDO(emailAddress);

                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                emailAddressValidator.ValidateAndThrow(emailAddressDO);

                if (meetingInfo.Trim().Length < 30)
                    return Json(new { Message = "Meeting information must have at least 30 characters" });

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                    string userId = _br.Generate(uow, emailAddress, meetingInfo, "SubmitsViaCreateManuallyBooker", subject);
                    return new JsonResult() { Data = new { Message = "A new booking requested created!", Result = "Success"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch (ValidationException ex)
            {
                return new JsonResult() { Data = new { Message = "You need to provide a valid Email Address.", Result = "Failure" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error processing a home page try it out form schedule me", ex);
                return new JsonResult() { Data = new { Message = "Something went wrong. Sorry about that", Result = "Failure" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [AllowAnonymous]
        public ActionResult CreateViaHomePage(string emailAddress, string meetingInfo)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                    string userId = _br.Generate(uow, emailAddress, meetingInfo, "SubmitsViaTryItOut", "");
                    return new JsonResult() { Data = new { Message = "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be", UserID = userId }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
            }
            catch (Exception e)
            {
                return new JsonResult() { Data = new { Message = "Sorry! Something went wrong. Alpha software..." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
        
        public ActionResult DisplayOneOffEmailForm(int bookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);

                var emailController = new EmailController();

                var br = new BookingRequest();
                var emailAddresses = br.ExtractEmailAddresses(bookingRequestDO);
                var userID = this.GetUserId();

                var currCreateEmailVM = new CreateEmailVM
                {
                    AddressBook = emailAddresses.ToList(),
                    Subject = bookingRequestDO.Subject,
                    SubjectEditable = false,

                    HeaderText = "Send an email",
                    BodyPromptText = "Enter some text for your recipients",
                    Body = "",
                };
                return emailController.DisplayEmail(Session, currCreateEmailVM,
                    (subUow, emailDO) =>
                    {
                        var sendingUser = subUow.UserRepository.GetByKey(userID);
                        subUow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, ObjectFactory.GetInstance<IConfigRepository>().Get("SimpleEmail_template"));

                        var currBookingRequest = new BookingRequest();
                        currBookingRequest.AddExpectedResponseForBookingRequest(subUow, emailDO, bookingRequestID);

                        emailDO.FromID = sendingUser.EmailAddressID;
                        emailDO.FromName = sendingUser.DisplayName + " via Kwasant";
                        subUow.SaveChanges();
                        return Json(true);
                    });
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

        // GET: /BookingRequest/ShowAwaitingResponse
        public ActionResult ShowAwaitingResponse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAwaitingResponse()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string currBooker = this.GetUserId();
                var jsonResult = Json(_jsonPackager.Pack(_br.GetAwaitingResponse(uow, currBooker).Select(e => new
                {
                    id = e.Id,
                    subject = e.Subject,
                    fromAddress = e.From.Address,
                    dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                    body =
                        e.HTMLText.Trim().Length > 400
                            ? e.HTMLText.Trim().Substring(0, 400)
                            : e.HTMLText.Trim()
                })
                    ));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/FindBookingRequest
        public ActionResult FindBookingRequest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(string queryPeriod, bool includeInvalid, int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_jsonPackager.Pack(_br.Search(uow, queryPeriod, includeInvalid, id).Select(e => new
                {
                    id = e.Id,
                    subject = e.Subject,
                    fromAddress = e.From.Address,
                    dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                    body =
                        e.HTMLText.Trim().Length > 400
                            ? e.HTMLText.Trim().Substring(0, 400)
                            : e.HTMLText.Trim()
                })
                ));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
    }
}