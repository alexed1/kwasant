using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;


namespace KwasantTest.Services
{
    [TestFixture]
    public class EmailTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        private Email _curEmail;
        private EventDO _curEventDO;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            //_bookingRequestRepo = new BookingRequestRepository(_uow);
            _fixture = new FixtureData(_uow);
            _curEmail = new Email(_uow, new EmailDO());
            _curEventDO = new EventDO();
        }

       

        [Test]
        [Category("Email")]
        public void CanCreateStandardInviteEmail()
        {
            //SETUP  
            _curEventDO = _fixture.TestEvent2();
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;
           
            
            //EXECUTE
            EmailDO _curEmailDO = _curEmail.CreateStandardInviteEmail(_curEventDO);
            
            //VERIFY
            Assert.AreEqual(_curEmailDO.Subject,  expectedSubject);
            
        }

        [Test]
        [Category("Email")]
        public void CreateSIE_FailsIfInvalidEventInput()
        {
            //SETUP
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;
            EmailDO _curEmailDO; 
            
            //EXECUTE
  
            //VERIFY
            Assert.Throws<ValidationException>(() =>
                {
                    _curEmailDO = _curEmail.CreateStandardInviteEmail(_curEventDO);
                }

            );

        }

        [Test]
        [Category("Email")]
        public void CanSendSIE_MANUALTEST()
        {
            //SETUP  
            _curEventDO = _fixture.TestEvent2();
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;


            //EXECUTE
            EmailDO _curEmailDO = _curEmail.CreateStandardInviteEmail(_curEventDO);
            _curEmail.Send(_curEmailDO);

            //VERIFY
            //implement a technique later to go and pull from the email inbox and verify. for now, verify by hand.

        }

   
       
    }
}
