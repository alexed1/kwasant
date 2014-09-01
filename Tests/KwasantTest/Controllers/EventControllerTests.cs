using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Fixtures;
using KwasantWeb.App_Start;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class EventControllerTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        private EventDO _curEventDO;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixture = new FixtureData();

            AutoMapperBootStrapper.ConfigureAutoMapper();
          
        }
        //create real eventcontroller with real Attendee and Event (later should isolate these with mocks but need to test integration now)
        //create mock event with id 1 and save it to the mockdb
        //create eventvm with a new attendee string with an email, and with id 1 to match the event in the mockdb
        //call processconfirmedevent
        //should complete without an exception
        [Test, Category("EventController")]
        public void CanProcessConfirmedEvent()
        {
            EventDO testEvent = _fixture.TestEvent1();
            testEvent.EventStatus = EventState.ProposedTimeSlot; //a convenient way to get DispatchInvitations to do nothing. 
            _uow.EventRepository.Add(testEvent);
            _uow.SaveChanges();

            EventController curEventController = new EventController();
            var curEventVM = new EventVM
            {
                Id=testEvent.Id
            };
            curEventVM.Attendees = "newattendee@kwasant.net";
            curEventController.ProcessChangedEvent(curEventVM, ConfirmationStatus.Confirmed);


        }


    }
}
