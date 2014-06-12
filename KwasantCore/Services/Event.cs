using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using IEvent = Data.Interfaces.IEvent;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public EventDO Create(EventDO curEventDO, IUnitOfWork uow)
        {
            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) &&
                                  curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO = AddAttendee(bookingRequestDO.User, curEventDO);
            curEventDO.Status = "Instantiated";

            return curEventDO;
        }


        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes

        public void Update(IUnitOfWork uow, EventDO eventDO)
        {
            switch (eventDO.Status)
            {
                case "Instantiated":
                    eventDO.Status = "Undispatched";
                    //this is a new event that was instantiated and stored during #Create

                    break;
                case "Dispatched":
                case "Undispatched":
                    //Dispatched means this event was previously created. This is a standard event change. We need to figure out what kind of update message to send
                    //Undispatched is the uncommon case where the event is created, queued up with outbound emails ready to go, and then the event gets changed
                    
                    if (EventHasChanged(uow, eventDO))
                    {
                        //mark the new attendees with status "New"
                    }
                    else
                    {
                        //create an EventChangeRecord to store the change information to put into the update email
                        //mark all attendees with status "NeedsUpdate"
                    }
                    break;
            }
        }

        private void SendEventUpdates(EventDO curEvent, EventDO newEventData)
        {
            // if eventtimes have changed
            //send an event update email to attendees
        }

        private bool EventHasChanged(IUnitOfWork uow, EventDO eventDO)
        {
            var changedProperties = uow.Db.GetEntityModifications(eventDO);
            //determine if changes warrant an update message
            //return true or false
            return false;
        }


        public EventDO AddAttendee(UserDO curUserDO, EventDO curEvent)
        {
            var curAttendee = new Attendee();
            var curAttendeeDO = curAttendee.Create(curUserDO);
            curEvent.Attendees.Add(curAttendeeDO);
            return curEvent;
        }
    }

    public class RazorViewModel
    {
        public bool IsAllDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String Summary { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public List<RazorAttendeeViewModel> Attendees { get; set; }

        public RazorViewModel(EventDO ev)
        {
            IsAllDay = ev.IsAllDay;
            StartDate = ev.StartDate;
            EndDate = ev.EndDate;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
            Attendees =
                ev.Attendees.Select(
                    a => new RazorAttendeeViewModel {Name = a.Name, EmailAddress = a.EmailAddress.Address}).ToList();
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}