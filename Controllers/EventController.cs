using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;


namespace KwasantWeb.Controllers
{
    public class EventController : Controller
    {
      
        //Renders a form to accept a new event
        public ActionResult New(int bookingRequestID, string start, string end)
        {

            //load event from Event service
            var curEvent = new Event();
            var curEventDO = curEvent.Create(bookingRequestID, start, end);

            //put it in a view model to hand to the view
            var curEventVM = Mapper.Map<EventDO, EventViewModel>(curEventDO);

            //construct a Calendar view model for this Calendar View 
            return View("~/Views/Event/Edit.cshtml", curEventVM);
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

        public ActionResult SubmitChange(EventViewModel eventViewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userRow = uow.UserRepository.GetByKey(User.Identity.GetUserId());
                EventDO eventDO = eventViewModel.Id == 0
                    ? new EventDO { CreatedByID = userRow.Id, CreatedBy = userRow }
                    : uow.EventRepository.GetByKey(eventViewModel.Id);

                Mapper.Map(eventViewModel, eventDO);

                var eve = new Event();

                eve.ManageAttendeeList(uow, eventDO, eventViewModel.Attendees);
                    
                if (eventViewModel.Id == 0)
                    uow.EventRepository.Add(eventDO);

                eve.Dispatch(uow, eventDO);

                uow.SaveChanges();
            }

            return JavaScript(SimpleJsonSerializer.Serialize(true));
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
