using System;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class CalendarControllerTests
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
            //_curEventDO = new EventDO();
        }

       

        [Test]
        [Category("Email")]
        public void CanCreateStandardInviteEmail()
        {
            //SETUP  
            _curEventDO = _fixture.TestEvent2();
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;
           
            
            //EXECUTE
            var curEmail = new Email(_uow, _curEventDO);
            curEmail.GenerateInvitation(_curEventDO);
            //VERIFY
            //Assert.AreEqual(_curEmailDO.Subject,  expectedSubject);

        }

      
   
       
    }
}
