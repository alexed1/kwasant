using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                throw new HttpException(400, "Booking request not found");

            if (TempData["requestInfo"] == null)
                GetRequestInfo(id);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestDORepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(id);

                if (bookingRequestDO == null)
                    throw new HttpException(400, "Booking request not found");

                var linkedNegotiationID =
                    bookingRequestDO.Negotiations.Where(
                        n =>
                            n.NegotiationState == NegotiationState.InProcess ||
                            n.NegotiationState == NegotiationState.AwaitingClient)
                        .Select(n => (int?)n.Id)
                        .FirstOrDefault();

                CalendarShowVM calWidget = new CalendarShowVM
                {
                    LinkedNegotiationID = linkedNegotiationID,
                    LinkedCalendarIds = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).ToList(),

                    //In the future, we won't need this - the 'main' calendar will be picked by the booker
                    ActiveCalendarId = bookingRequestDO.Calendars.Select(calendarDO => calendarDO.Id).FirstOrDefault()
                };

                return View(new DashboardShowVM
                {
                    CalendarVM = calWidget,
                    ResolvedNegotiations = bookingRequestDO.Negotiations.Where(n => n.NegotiationState == NegotiationState.Resolved).Select(n => new DashboardNegotiationVM
                    {
                        Id = n.Id,
                        Name = n.Name
                    }).ToList(),
                    BookingRequestVM = TempData["requestInfo"] as BookingRequestAdminVM
                });
            }
        }

        public void GetRequestInfo(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                
                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                var attachmentInfo = String.Join("<br />",
                            bookingRequest.Attachments.Select(
                                attachment =>
                                "<a href='" + String.Format(fileViewURLStr, attachment.Id) + "' target='" +
                                attachment.OriginalName + "'>" + attachment.OriginalName + "</a>"));

                string booker = "none";
                string bookerId = uow.BookingRequestRepository.GetByKey(bookingRequestId).BookerID;
                if (bookerId != null)
                {
                    booker = uow.UserRepository.GetByKey(bookerId).EmailAddress.Address;
                }

                var emails = new List<EmailDO>();
                emails.Add(bookingRequest);
                emails.AddRange(bookingRequest.ConversationMembers);

                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    Conversations = emails.OrderBy(c => c.DateReceived).Select(e => new ConversationVM
                    {
                        Header = String.Format("From: {0}  {1}", e.From.Address, e.DateReceived.TimeAgo()),
                        Body = e.HTMLText
                    }).ToList(),
                    Subject = bookingRequest.Subject,
                    BookingRequestId = bookingRequestId,
                    EmailTo = String.Join(", ", bookingRequest.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", bookingRequest.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", bookingRequest.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    Booker = booker
                };
                TempData["requestInfo"] = bookingInfo;
            }
        }
	}
}