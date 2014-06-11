using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Infrastructure;
using Data.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Managers.IdentityManager;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
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
            using (var uow = UOW())
            {
                //unpack the form data into an EventDO 
                EventDO submittedEventData = new EventDO();
                submittedEventData.BookingRequestID = bookingRequestID;
                submittedEventData.StartDate = DateTime.Parse(start);
                submittedEventData.EndDate = DateTime.Parse(end);
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
            using (var uow = UOW())
            {
                //unpack view model
                EventDO submittedEventDO = Mapper.Map<EventDO>(eventVM);
                submittedEventDO.Attendees = _attendee.ConvertFromString(uow, eventVM.Attendees);
                if (eventVM.Id == 0)
                {
                    throw new ApplicationException("event should have been created and saved in #new, so Id should not be zero");
                }
                else
                {
                    _event.Update(uow, submittedEventDO);
                  //  uow.SaveChanges(); FIX THIS 
            

                }       

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