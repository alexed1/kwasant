using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = Roles.Booker)]
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                throw new HttpException(400, "Booking request not found");

            if (TempData["requestInfo"] == null)
            {
                var data = new EmailController().GetInfo(id);
                TempData["requestInfo"] = data.Model;
            }

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

        
	}

                var journaling = uow.FactRepository.GetJournalingForBookingRequest(bookingRequestId).ToList();

                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    ConversationMembers = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestId).Select(e => e.Id).ToList(),
                    BookingRequestId = bookingRequestId,
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
                    VerbalisedHistory = journaling,
                    EmailTo = String.Join(", ", curEmail.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", curEmail.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", curEmail.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    Booker = booker
                };
                TempData["requestInfo"] = bookingInfo;
            }
        }
	}
}