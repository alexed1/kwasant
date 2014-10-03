using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    ConversationMembers = uow.EmailRepository.GetQuery().Where(e => e.ConversationId == bookingRequestDO.Id).Select(e => e.Id).ToList(),
                    BookingRequestId = bookingRequestDO.Id,
                    CurEmailData = uow.EmailRepository.GetByKey(id)
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