using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Calendar = KwasantCore.Services.Calendar;
using ViewModel.Models;
using Data.Entities;
using DayPilot.Web.Mvc.Json;
using Data.Repositories;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Interfaces;

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

        public ActionResult New(int bookingRequestID, string start, string end)
        {
            return View("~/Views/Event/Edit.cshtml", EventViewModel.NewEventOnBookingRequest(bookingRequestID, start, end));
        }

        public ActionResult Edit(int eventID)
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

                uow.SaveChanges();
                
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
                return View("~/Views/Event/ConfirmEventEdits.cshtml", evm);
            }
        }

        /// <summary>
        /// This method creates a template eventDO which we store. This event is presented to the user to review & confirm changes. If they confirm, Confirm(FormCollection form) is invoked
        /// </summary>
        /// <param name="eventViewModel"></param>
        /// <returns></returns>
        public ActionResult ConfirmEventEdits(EventViewModel eventViewModel)
        {
            return View(eventViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(EventViewModel eventViewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EventDO eventDO = eventViewModel.Id == 0
                    ? new EventDO { CreatedByID = User.Identity.GetUserId() }
                    : uow.EventRepository.GetByKey(eventViewModel.Id);

                eventViewModel.FillEventDO(uow, eventDO);
                if (eventViewModel.Id == 0)
                {
                    uow.EventRepository.Add(eventDO);
                }

                uow.SaveChanges();
            }
            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }
	}
}