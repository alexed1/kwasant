using System.Diagnostics;
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
            var curEventVM = new EventViewModel(curEventDO);

            //construct a Calendar view model for this Calendar View 
            return View("~/Views/Event/Edit.cshtml", curEventVM);
        }

        //the viewmodel has a confirmation boolean that is set on the client when the user approves the data.
        public ActionResult ProcessSubmittedEvent(EventViewModel eventViewModel)
        {
       
            if  (false)  //(eventViewModel.Confirmed == false) TO DO: fix this so confirmation works properly again.
                return View("ConfirmChanges", eventViewModel);
            else
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                                  
                    BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(eventViewModel.BookingRequestID);
                    EventDO eventDO = Mapper.Map<EventDO>(eventViewModel);
                    eventDO.CreatedBy = bookingRequestDO.User;

                    Event curEvent = new Event();
                    curEvent.ManageAttendeeList(uow, eventDO, eventViewModel.Attendees);

                    if (eventViewModel.Id == 0)
                    {
                        uow.EventRepository.Add(eventDO);
                    }

                    uow.SaveChanges();
                }

                return RedirectToAction( "Index", "Calendar");
            }
        }






    }
}
