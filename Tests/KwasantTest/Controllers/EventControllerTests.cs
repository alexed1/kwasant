using System;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class EventControllerTests : BaseTest
    {
        //create real eventcontroller with real Attendee and Event (later should isolate these with mocks but need to test integration now)
        //create mock event with id 1 and save it to the mockdb
        //create eventvm with a new attendee string with an email, and with id 1 to match the event in the mockdb
        //call processconfirmedevent
        //should complete without an exception
        [Test, Category("EventController")]
        public void CanProcessConfirmedEvent()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var user = fixture.TestUser1();
                EventDO testEvent = fixture.TestEvent1();
                testEvent.BookingRequest = fixture.TestBookingRequest1();
                testEvent.CreatedBy = user;

                testEvent.EventStatus = EventState.ProposedTimeSlot;
                    //a convenient way to get DispatchInvitations to do nothing. 
                uow.EventRepository.Add(testEvent);
                uow.SaveChanges();

                EventController curEventController = new EventController();
                var curEventVM = new EventVM
                {
                    Id = testEvent.Id,
                    Summary = String.Empty
                };
                curEventVM.Attendees = "newattendee@kwasant.net";
                curEventController.ProcessChangedEvent(curEventVM, ConfirmationStatus.Confirmed, false);
            }

        }


    }
}
