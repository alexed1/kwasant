using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;


namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class EventController : KController
    {
        private Event _event;
        private Attendee _attendee;

        public EventController()
        {
            _event = new Event();
            _attendee = new Attendee();
        }

        public ActionResult New(int bookingRequestID, int calendarID, string start, string end)
        {
            using (var uow = GetUnitOfWork())
            {
                var createdEvent = CreateNewEvent(uow, bookingRequestID, calendarID, start, end);
                _event.Create(createdEvent, uow);

                uow.EventRepository.Add(createdEvent);
                uow.SaveChanges();

                //put it in a view model to hand to the view
                var curEventVM = Mapper.Map<EventDO, EventVM>(createdEvent);

                //construct a Calendar view model for this Calendar View 
                return View("~/Views/Event/Edit.cshtml", curEventVM);
            }
        }

        public ActionResult NewTimeSlot(int calendarID, string start, string end)
        {
            using (var uow = GetUnitOfWork())
            {
                var createdEvent = CreateNewEvent(uow, null, calendarID, start, end);

                createdEvent.CreatedByID = uow.CalendarRepository.GetByKey(calendarID).OwnerID;
                createdEvent.EventStatus = EventState.ProposedTimeSlot;
                
                uow.EventRepository.Add(createdEvent);
                //And now we merge changes
                MergeTimeSlots(uow, createdEvent);
                uow.SaveChanges();

                return JavaScript(SimpleJsonSerializer.Serialize(true));
            }
        }

        private void MergeTimeSlots(IUnitOfWork uow, EventDO updatedEvent)
        {
            //We want to merge existing events so we get continous blocks whenever they're 'filled in'.

            //We want to merge in the following situtations:

            //We have an existing event window ending at the same time as our new window starts.
            //This looks like this:
            //[Old--Event][New--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window start at the same time as our new window ends.
            //This looks like this:
            //[New--Event][Old--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window that partially overlaps our event from the beginning
            //This looks like this:
            //[Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //[New-----Event]

            //We have an existing event window that partially overlaps our event from the end
            //This looks like this:
            //      [Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //   [New-----Event]

            //We have an existing event that's fully contained within our event window
            //This looks like this:
            //    [Old--Event]
            //[New-----------Event]
            //With a merge result of this:
            //[New-----------Event]

            //All overlapping event windows are deleted from the system, with our new/modified event being expanded to encompass the overlaps.


            var overlaps =
                uow.EventRepository.GetQuery()
                    .Where(ew =>
                        ew.Id != updatedEvent.Id && ew.CalendarID == updatedEvent.CalendarID &&
                            //If the existing event starts at or before our start date, and ends at or after our start date
                        ((ew.StartDate <= updatedEvent.StartDate && ew.EndDate >= updatedEvent.StartDate) ||
                            //If the existing event starts at or after our end date, and ends at or before our end date
                         (ew.StartDate <= updatedEvent.EndDate && ew.EndDate >= updatedEvent.EndDate) ||
                            //If the existing event is entirely within our new date
                         (ew.StartDate >= updatedEvent.StartDate && ew.EndDate <= updatedEvent.EndDate)
                         )).ToList();

            //We want to get the min/max start dates _including_ our new event window
            var fullSet = overlaps.Union(new[] { updatedEvent }).ToList();

            var newStart = fullSet.Min(ew => ew.StartDate);
            var newEnd = fullSet.Max(ew => ew.EndDate);

            //Delete all overlaps
            foreach (var overlap in overlaps)
                uow.EventRepository.Remove(overlap);

            //Potentially expand our new/modified event window
            updatedEvent.StartDate = newStart;
            updatedEvent.EndDate = newEnd;
        }

        //Renders a form to accept a new event
        private EventDO CreateNewEvent(IUnitOfWork uow, int? bookingRequestID, int calendarID, string start, string end)
        {
            if (!start.EndsWith("z"))
                throw new ApplicationException("Invalid date time");
            if (!end.EndsWith("z"))
                throw new ApplicationException("Invalid date time");

            //unpack the form data into an EventDO 
            EventDO createdEvent = new EventDO();
            createdEvent.BookingRequestID = bookingRequestID;
            createdEvent.CalendarID = calendarID;            
            createdEvent.StartDate = DateTime.Parse(start, CultureInfo.InvariantCulture, 0).ToUniversalTime();
            createdEvent.EndDate = DateTime.Parse(end, CultureInfo.InvariantCulture, 0).ToUniversalTime();

            createdEvent.IsAllDay = createdEvent.StartDate.Equals(createdEvent.StartDate.Date) && createdEvent.StartDate.AddDays(1).Equals(createdEvent.EndDate);

            return createdEvent;
        }

        public ActionResult Edit(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                return View(Mapper.Map<EventDO, EventVM>(eventDO));
            }
        }

        public ActionResult ConfirmDelete(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                if (eventDO != null)
                    uow.EventRepository.Remove(eventDO);

                uow.SaveChanges();

                return JavaScript(SimpleJsonSerializer.Serialize(true));
            }
        }

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd, bool requiresConfirmation = true, bool mergeEvents = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!newStart.EndsWith("z"))
                    throw new ApplicationException("Invalid date time");
                if (!newEnd.EndsWith("z"))
                    throw new ApplicationException("Invalid date time");

                var eventDO = uow.EventRepository.GetByKey(eventID);

                var evm = Mapper.Map<EventDO, EventVM>(eventDO);

                evm.StartDate = DateTime.Parse(newStart, CultureInfo.InvariantCulture, 0).ToUniversalTime();
                evm.EndDate = DateTime.Parse(newEnd, CultureInfo.InvariantCulture, 0).ToUniversalTime();

                if(requiresConfirmation)
                    return View("~/Views/Event/ConfirmChanges.cshtml", evm);
                
                return ProcessConfirmedEvent(evm, mergeEvents);
            }
        }

        public ActionResult ConfirmChanges(EventVM eventVM)
        {
            return View(eventVM);
        }

        //processes events that have been entered into the form and confirmed
        public ActionResult ProcessConfirmedEvent(EventVM eventVM, bool mergeEvents = false)
        {
            using (var uow = GetUnitOfWork())
            {
                if (eventVM.Id == 0)
                    throw new ApplicationException("event should have been created and saved in #new, so Id should not be zero");

                EventDO existingEvent = uow.EventRepository.GetByKey(eventVM.Id);

                if (existingEvent == null)
                    throw new ApplicationException("should not be able to call this Update method with an ID that doesn't match an existing event");

                Mapper.Map(eventVM, existingEvent);
                existingEvent.SyncStatus = EventSyncState.SyncWithExternal;
                _attendee.ManageEventAttendeeList(uow, existingEvent, eventVM.Attendees);

                _event.Process(uow, existingEvent);

                if (mergeEvents)
                    MergeTimeSlots(uow, existingEvent);
                
                uow.SaveChanges();

                return JavaScript(SimpleJsonSerializer.Serialize(true));
            }
        }


        public ActionResult DeleteEvent(int eventID, bool requiresConfirmation = true)
        {
            if (requiresConfirmation)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                    return View(Mapper.Map<EventDO, EventVM>(eventDO));
                }
            }

            return ConfirmDelete(eventID);
        }
    }
}