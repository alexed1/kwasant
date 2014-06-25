using System;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.CommunicationManager;
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
            curEventDO.StateID = EventState.Booking;

            return curEventDO;
        }

        public EventDO Create(IUnitOfWork uow, int bookingRequestID, string startDate, string endDate)
        {
            var curEventDO = new EventDO();
            curEventDO.StartDate = DateTime.Parse(startDate);
            curEventDO.EndDate = DateTime.Parse(endDate);
            curEventDO.BookingRequestID = bookingRequestID;
            return Create(curEventDO, uow);
        }


        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes
        public void Process(IUnitOfWork uow, EventDO eventDO)
        {
            eventDO.StateID = EventState.ReadyForDispatch;
            new CommunicationManager().DispatchInvitations(uow, eventDO);
        }
     
        public EventDO AddAttendee(UserDO curUserDO, EventDO curEvent)
        {
            var curAttendee = new Attendee();
            var curAttendeeDO = curAttendee.Create(curUserDO);
            curEvent.Attendees.Add(curAttendeeDO);
            return curEvent;
        }
    }
}