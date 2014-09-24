using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantWeb.ViewModels;
using StructureMap;

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
                var curEmail = uow.EmailRepository.GetByKey(id);
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
                    booker = uow.UserRepository.GetByKey(bookerId).EmailAddress.Address;
                }

                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
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

                return View(new DashboardShowVM
                {
                    CalendarVM = calWidget,
                    BookingRequestVM = bookingInfo
                });
            }
        }
	}
}