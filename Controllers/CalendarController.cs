using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Services;
using KwasantWeb.Controllers.DayPilot;
using StructureMap;
using UtilitiesLib;
using Calendar = KwasantCore.Services.Calendar;
using ViewModel.Models;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {
        #region "Action"

        private IUnitOfWork _uow;



        public CalendarController()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow = uow; //clean this up finish de-static work

        }




        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

           
            IBookingRequestRepository bookingRequestRepository = new BookingRequestRepository(_uow);
            BookingRequestDO = bookingRequestRepository.GetByKey(id);
            if (BookingRequestDO == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendar curCalendar = new Calendar(_uow);
            curCalendar.LoadBookingRequest(BookingRequestDO);
            Session["CalendarServices"] = curCalendar;
            return View(BookingRequestDO);
        }

        private Calendar Calendar
        {
            get
            {
                return Session["CalendarServices"] as Calendar;
            }
            set
            {
                Session["CalendarServices"] = value;
            }
        }

        private BookingRequestDO BookingRequestDO
        {
            get
            {
                return Session["BookingRequestDO"] as BookingRequestDO;
            }
            set
            {
                Session["BookingRequestDO"] = value;
            }
        }

        public ActionResult Day()
        {
            return new DayPilotCalendarControl(Calendar).CallBack(this);
        }


        public ActionResult Month()
        {
            return new DayPilotMonthControl(Calendar).CallBack(this);
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

        public ActionResult Backend()
        {
            return new DayPilotCalendarControl(Calendar).CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new DayPilotNavigatorControl().CallBack(this);
        }

        public ActionResult New(string start, string end)
        {
            EventDO eventDO = new EventDO
            {
                StartDate = DateTime.Parse(start),
                EndDate = DateTime.Parse(end),
                BookingRequest = BookingRequestDO,
            };
            //If there's no time component for the start date (ie starting at midnight), and the end is exactly 1 day ahead, it's an all-day-event
            if (eventDO.StartDate.Equals(eventDO.StartDate.Date) &&
                eventDO.StartDate.AddDays(1).Equals(eventDO.EndDate))
                eventDO.IsAllDay = true;

            eventDO.Attendees = new List<AttendeeDO>
            {
                new AttendeeDO
                {
                    EmailAddress = BookingRequestDO.From.Address,
                    Name = BookingRequestDO.From.Name,
                    Event = eventDO
                }
            };

            return View("~/Views/Calendar/Open.cshtml", eventDO);
        }

        public ActionResult Open(int eventID)
        {
            return View(
                Calendar.GetEvent(eventID)
                );
        }

        public ActionResult DeleteEvent(int eventID)
        {
            EventDO actualEventDO = Calendar.GetEvent(eventID);
            return View(actualEventDO);
        }

        public ActionResult ConfirmDelete(int eventID)
        {
            Calendar.DeleteEvent(eventID);
            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd)
        {
            //This is a fake event that will be thrown away if Confirm() is not called
            EventDO eventDO = new EventDO();
            eventDO.EventID = eventID;
            EventDO actualEventDO = Calendar.GetEvent(eventID);
            eventDO.CopyFrom(actualEventDO);

            DateTime newStartDT = DateTime.Parse(newStart);
            DateTime newEndDT = DateTime.Parse(newEnd);

            eventDO.StartDate = newStartDT;
            eventDO.EndDate = newEndDT;

            string key = Guid.NewGuid().ToString();
            Session["FakedEvent_" + key] = eventDO;
            return View("~/Views/Calendar/ProcessCreateEvent.cshtml", new ConfirmEvent
            {
                Key = key,
                EventDO = eventDO
            });
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
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult ProcessCreateEvent(CalendarViewModel calendarViewModel)
        {
            //This is a fake event that will be thrown away if Confirm() is not called
            EventDO eventDO = new EventDO
            {
                EventID = calendarViewModel.EventID,
                IsAllDay = GetCheckFlag(calendarViewModel.IsAllDay),
                StartDate = calendarViewModel.DateStart,
                EndDate = calendarViewModel.DateEnd,
                Location = calendarViewModel.Location,
                Status = calendarViewModel.Status,
                Transparency = calendarViewModel.TransparencyType,
                Class = calendarViewModel.Class,
                Description = calendarViewModel.Description,
                Priority = calendarViewModel.Priority,
                Sequence = calendarViewModel.Sequence,
                Summary = calendarViewModel.Summary,
                Category = calendarViewModel.Category
            };

            ManageAttendees(eventDO, calendarViewModel.Attendees);

            string key = Guid.NewGuid().ToString();
            Session["FakedEvent_" + key] = eventDO;
            return View(
                new ConfirmEvent
                {
                    Key = key,
                    EventDO = eventDO
                }
            );
        }

        //Manages adds/deletes and persists of attendees.
        private void ManageAttendees(EventDO eventDO, string attendeesStr)
        {
            //Load the known attendee list for this event
            List<AttendeeDO> originalAttendees;
            if (eventDO.EventID != 0)
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.EventID);
                originalAttendees = new List<AttendeeDO>(oldEvent.Attendees);
            }
            else
            {
                originalAttendees = new List<AttendeeDO>();
            }

            //create the list that will merge in the changes
            List<AttendeeDO> newAttendees = new List<AttendeeDO>();
            foreach (string email in attendeesStr.Split(','))
            {
                if (String.IsNullOrEmpty(email))
                    continue;

                
                List<AttendeeDO> sameAttendees = originalAttendees.Where(oa => oa.EmailAddress == email).ToList();
                if (sameAttendees.Any())
                {
                    //take all of the attendees that were already associated with the event and add them to the merged list
                    newAttendees.AddRange(sameAttendees);
                }
                else
                {
                    //create a new attendee and add it
                    newAttendees.Add(new AttendeeDO
                    {
                        EmailAddress = email
                    });
                }
            }

            //Delete any attendees that are no longer part of the list
            List<AttendeeDO> attendeesToDelete = originalAttendees.Where(originalAttendee => !newAttendees.Select(a => a.EmailAddress).Contains(originalAttendee.EmailAddress)).ToList();
            if (attendeesToDelete.Any())
            {
                AttendeeRepository attendeeRepo = new AttendeeRepository(Calendar.UnitOfWork);
                foreach (AttendeeDO attendeeToDelete in attendeesToDelete)
                    attendeeRepo.Remove(attendeeToDelete);
            }
            eventDO.Attendees = newAttendees;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(ProcessCreateEventViewModel processCreateEventViewModel)
        {

            EventDO eventDO = Session["FakedEvent_" + processCreateEventViewModel.Key] as EventDO;
            if (eventDO.EventID == 0)
            {
                Calendar.AddEvent(eventDO);
            }
            else
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.EventID);
                oldEvent.CopyFrom(eventDO);
                eventDO = oldEvent;
            }

            eventDO.BookingRequest = BookingRequestDO;

            var curEvent = new Event();
            curEvent.Dispatch(eventDO);
           

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        #endregion "Action"

        public class ConfirmEvent
        {
            public string Key;
            public EventDO EventDO;
        }
    }
}
