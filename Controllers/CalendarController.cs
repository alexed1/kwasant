using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Services;
using KwasantWeb.Controllers.DayPilot;
using KwasantWeb.ViewModels;
using StructureMap;
using ViewModel.Models;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {
        #region "Action"

        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestRepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(id);
                if (bookingRequestDO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                return View(bookingRequestDO);
            }
        }

        public ActionResult Day(int bookingRequestID)
        {
            return new DayPilotCalendarControl(bookingRequestID).CallBack(this);
        }

        public ActionResult Month(int bookingRequestID)
        {
            return new DayPilotMonthControl(bookingRequestID).CallBack(this);
        }  

        public ActionResult Rtl()
        {
            return View();
        }
        public ActionResult Columns50()
        {
            return View();
        }
        public ActionResult Height100Pct()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }

        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult ThemeWhite()
        {
            return View();
        }

        public ActionResult ThemeTraditional()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }

        public ActionResult TimeHeaderCellDuration()
        {
            return View();
        }

        public ActionResult ActiveAreas()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult HeaderAutoFit()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult EventArrangement()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult Today()
        {
            return View();
        }

        public ActionResult DaysResources()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult DayRange()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult AutoHide()
        {
            return View();
        }

        public ActionResult GoogleLike()
        {
            return View();
        }

        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult ThemeGreenWithBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult Outlook2000()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult Backend(int bookingRequestID)
        {
            return new DayPilotCalendarControl(bookingRequestID).CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new DayPilotNavigatorControl().CallBack(this);
        }

        public ActionResult New(int bookingRequestID, string start, string end)
        {
            return View("~/Views/Calendar/Open.cshtml", EventViewModel.NewEventOnBookingRequest(bookingRequestID, start, end));
        }

        public ActionResult Open(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                return View(new EventViewModel(eventDO));
            }
        }

        public ActionResult DeleteEvent(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                return View(new EventViewModel(eventDO));
            }
        }

        public ActionResult ConfirmDelete(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                if (eventDO != null)
                    uow.EventRepository.Remove(eventDO);

                return JavaScript(SimpleJsonSerializer.Serialize("OK"));
            }
        }

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetByKey(eventID);
                var evm = new EventViewModel(eventDO)
                {
                    StartDate = DateTime.Parse(newStart),
                    EndDate = DateTime.Parse(newEnd)
                };
                return View("~/Views/Calendar/ProcessCreateEvent.cshtml", evm);
            }
        }

        private bool GetCheckFlag(String value)
        {
            bool blnCheck = false;

            blnCheck = value == "on" ? true : false;

            return blnCheck;
        }

        /// <summary>
        /// This method creates a template eventDO which we store. This event is presented to the user to review & confirm changes. If they confirm, Confirm(FormCollection form) is invoked
        /// </summary>
        /// <param name="eventViewModel"></param>
        /// <returns></returns>
        public ActionResult ProcessCreateEvent(EventViewModel eventViewModel)
        {
            ////This is a fake event that will be thrown away if Confirm() is not called
            //EventDO eventDO = new EventDO
            //{
            //    Id = calendarViewModel.EventID,
            //    IsAllDay = GetCheckFlag(calendarViewModel.IsAllDay),
            //    StartDate = calendarViewModel.DateStart,
            //    EndDate = calendarViewModel.DateEnd,
            //    Location = calendarViewModel.Location,
            //    Status = calendarViewModel.Status,
            //    Transparency = calendarViewModel.TransparencyType,
            //    Class = calendarViewModel.Class,
            //    Description = calendarViewModel.Description,
            //    Priority = calendarViewModel.Priority,
            //    Sequence = calendarViewModel.Sequence,
            //    Summary = calendarViewModel.Summary,
            //    Category = calendarViewModel.Category
            //};

            //ManageAttendees(eventDO, calendarViewModel.Attendees);

            //string key = Guid.NewGuid().ToString();
            //Session["FakedEvent_" + key] = eventDO;
            //return View(
            //    new ConfirmEvent
            //    {
            //        Key = key,
            //        EventDO = eventDO
            //    }
            //);
            return View(eventViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(EventViewModel eventViewModel)
        {
            var curEvent = new Event();
            //curEvent.Dispatch(eventDO);
           

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        #endregion "Action"

    }
}
