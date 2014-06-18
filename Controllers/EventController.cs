using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Managers.IdentityManager;
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

        //Renders a form to accept a new event
        public ActionResult New(int bookingRequestID, string start, string end)
        {
            using (var uow = GetUnitOfWork())
            {
                if (!start.EndsWith("z"))
                    start = start + "z";
                if (!end.EndsWith("z"))
                    end = end + "z";
                //unpack the form data into an EventDO 
                EventDO submittedEventData = new EventDO();
                submittedEventData.BookingRequestID = bookingRequestID;
                submittedEventData.StartDate = DateTime.Parse(start, CultureInfo.InvariantCulture, 0).ToUniversalTime();
                submittedEventData.EndDate = DateTime.Parse(end, CultureInfo.InvariantCulture, 0).ToUniversalTime();
                EventDO createdEvent = _event.Create(submittedEventData, uow);
                uow.EventRepository.Add(createdEvent);
                uow.SaveChanges();

                //put it in a view model to hand to the view
                var curEventVM = Mapper.Map<EventDO, EventViewModel>(createdEvent);

                //construct a Calendar view model for this Calendar View 
                return View("~/Views/Event/Edit.cshtml", curEventVM);
            }
        }


        public ActionResult Edit(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                return View(Mapper.Map<EventDO, EventViewModel>(eventDO));
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

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetByKey(eventID);
                var evm = Mapper.Map<EventDO, EventViewModel>(eventDO);
                evm.StartDate = DateTime.Parse(newStart);
                evm.EndDate = DateTime.Parse(newEnd);

                return View("~/Views/Event/ConfirmChanges.cshtml", evm);
            }
        }

        public ActionResult ConfirmChanges(EventViewModel eventViewModel)
        {
            return View(eventViewModel);
        }

        //processes events that have been entered into the form and confirmed
        public ActionResult ProcessConfirmedEvent(EventViewModel eventVM)
        {
            using (var uow = GetUnitOfWork())
            {
                if (eventVM.Id == 0)
                    throw new ApplicationException("event should have been created and saved in #new, so Id should not be zero");

                var existingEvent = uow.EventRepository.GetByKey(eventVM.Id);

                if (existingEvent == null)
                    throw new ApplicationException("should not be able to call this Update method with an ID that doesn't match an existing event");

                Mapper.Map(eventVM, existingEvent);
                _attendee.ManageAttendeeList(uow, existingEvent, eventVM.Attendees);

                _event.Update(uow, existingEvent);
                uow.SaveChanges();

                return JavaScript(SimpleJsonSerializer.Serialize(true));
            }
        }


        public ActionResult DeleteEvent(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                return View(Mapper.Map<EventDO, EventViewModel>(eventDO));
            }
        }
    }
}