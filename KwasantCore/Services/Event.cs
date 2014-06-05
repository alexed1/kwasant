using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Policy;
using System.Web.UI;
using AutoMapper;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Validators;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using Utilities;
using Encoding = System.Text.Encoding;
using IEvent = Data.Interfaces.IEvent;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public EventDO Create(EventDO curEventDO, IUnitOfWork uow)
        {
             curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) && curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate); ;          
             var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO = AddAttendee(bookingRequestDO.User, curEventDO);       
            curEventDO.Status = "Instantiated";

            return curEventDO;
        }
     

        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes

        public void Update(IUnitOfWork uow, EventDO newEventData)
            {
            //curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) && curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate); ;          
            EventDO curEventDO = uow.EventRepository.GetByKey(newEventData.Id);
          
            if (curEventDO == null)
                    throw new ApplicationException("should not be able to call this Update method with an ID that doesn't match an existing event");
            switch (curEventDO.Status)
            {
                case "Instantiated":

                    //this is a new event that was instantiated and stored during #Create

                    break;
                case "Dispatched":
                case "Undispatched":
                    //Dispatched means this event was previously created. This is a standard event change. We need to figure out what kind of update message to send
                    //Undispatched is the uncommon case where the event is created, queued up with outbound emails ready to go, and then the event gets changed
                    if (EventHasChanged(curEventDO, newEventData))
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
            
            //use the new information, mapping it to overwrite the old information
            Mapper.Map<EventDO, EventDO>(newEventData, curEventDO);
            //attendees must be handled manually. automapper can't currently do it
            curEventDO.Attendees = newEventData.Attendees;
            //if event times have changed, may need to send updates


            curEventDO = newEventData;


        }

        private void SendEventUpdates(EventDO curEvent, EventDO newEventData)
        {
            // if eventtimes have changed
           //send an event update email to attendees
            }

        private bool EventHasChanged(EventDO oldEventDO, EventDO newEventDO)
        {
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

        //this is no longer used because we don't want to do string processing when we can instead work with objects
        //now, the controller maps the incoming attendee strings into attendee objects early in the process.
        //Processes the incoming attendee information, which is currently just a comma delimited string
        public void ManageAttendeeList(IUnitOfWork uow, EventDO eventDO, string curAttendees)
        {
            var attendees = curAttendees.Split(',').ToList();

            var eventAttendees = eventDO.Attendees ?? new List<AttendeeDO>();
            var attendeesToDelete = eventAttendees.Where(attendee => !attendees.Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            foreach (var attendee in attendees.Where(att => !eventAttendees.Select(a => a.EmailAddress.Address).Contains(att)))
            {
                var newAttendee = new AttendeeDO
                {
                    EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee),
                    Event = eventDO,
                    EventID = eventDO.Id,
                    Name = attendee
                };
                uow.AttendeeRepository.Add(newAttendee);
            }
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
            Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel { Name = a.Name, EmailAddress = a.EmailAddress.Address}).ToList();
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}
