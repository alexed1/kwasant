using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Calendar = KwasantCore.Services.Calendar;
using ViewModel.Models;
using Data.Entities;
using DayPilot.Web.Mvc.Json;
using Data.Repositories;
using KwasantCore.Services;

namespace KwasantWeb.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/
        //public ActionResult Index()
        //{
        //    return View();
        //}
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
            if (BookingRequestDO.To != null)
            {
                eventDO.Attendees.AddRange(BookingRequestDO.To.Select(a => new AttendeeDO
                {
                    EmailAddress = a.Address,
                    Name = a.Name,
                    Event = eventDO
                }));
            }
            if (BookingRequestDO.CC != null)
            {
                eventDO.Attendees.AddRange(BookingRequestDO.CC.Select(a => new AttendeeDO
                {
                    EmailAddress = a.Address,
                    Name = a.Name,
                    Event = eventDO
                }));
            }
            if (BookingRequestDO.BCC != null)
            {
                eventDO.Attendees.AddRange(BookingRequestDO.BCC.Select(a => new AttendeeDO
                {
                    EmailAddress = a.Address,
                    Name = a.Name,
                    Event = eventDO
                }));
            }

            return View("~/Views/Event/Edit.cshtml", eventDO);
        }

        public ActionResult Edit(int eventID)
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
            eventDO.Id = eventID;
            EventDO actualEventDO = Calendar.GetEvent(eventID);
            eventDO.CopyFrom(actualEventDO);

            DateTime newStartDT = DateTime.Parse(newStart);
            DateTime newEndDT = DateTime.Parse(newEnd);

            eventDO.StartDate = newStartDT;
            eventDO.EndDate = newEndDT;

            string key = Guid.NewGuid().ToString();
            Session["FakedEvent_" + key] = eventDO;
            return View("~/Views/Event/ConfirmEventEdits.cshtml", new KwasantWeb.Controllers.CalendarController.ConfirmEvent
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
        public ActionResult ConfirmEventEdits(CalendarViewModel calendarViewModel)
        {
            //This is a fake event that will be thrown away if Confirm() is not called
            EventDO eventDO = new EventDO
            {
                Id = calendarViewModel.EventID,
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
                new KwasantWeb.Controllers.CalendarController.ConfirmEvent
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
            if (eventDO.Id != 0)
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.Id);
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
                AttendeeRepository attendeeRepo = Calendar.UnitOfWork.AttendeeRepository;
                foreach (AttendeeDO attendeeToDelete in attendeesToDelete)
                    attendeeRepo.Remove(attendeeToDelete);
            }
            eventDO.Attendees = newAttendees;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(ProcessCreateEventViewModel processCreateEventViewModel)
        {

            EventDO eventDO = Session["FakedEvent_" + processCreateEventViewModel.Key] as EventDO;
            if (eventDO.Id == 0)
            {
                Calendar.AddEvent(eventDO);
            }
            else
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.Id);
                oldEvent.CopyFrom(eventDO);
                eventDO = oldEvent;
            }

            eventDO.BookingRequest = BookingRequestDO;

            var curEvent = new Event();
            curEvent.Dispatch(eventDO);


            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        //public class ConfirmEvent
        //{
        //    public string Key;
        //    public EventDO EventDO;
        //}
	}
}