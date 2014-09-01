using Data.Entities;
using Data.Interfaces;
using FluentValidation;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class EventServiceTests
    {
        private FixtureData _fixture;
        private IEvent _event;
        private CommunicationManager _comm;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);

            _fixture = new FixtureData();
            _event = new Event();
            _comm = ObjectFactory.GetInstance<CommunicationManager>();
        }

        [Test]
        [Category("Event")]
        public void CanGetOrignatorNameWithValidEmailAddress()
        {
            EventDO curEvent = _fixture.TestEvent2();

            string originator = _comm.GetOriginatorName(curEvent);
            Assert.AreNotEqual(originator, null);
        }

        //if first name and last name are null, but email address has a name, return the name
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressName()
        {
            EventDO curEvent = _fixture.TestEvent2();
            UserDO curOriginator = curEvent.CreatedBy;
            curOriginator.FirstName = null;
            curOriginator.LastName = null;
            curOriginator.EmailAddress.Name = "John Smallberries";
            string originator = _comm.GetOriginatorName(curEvent);
            Assert.AreEqual(originator, curOriginator.EmailAddress.Name);
        }

        //if first name and last name are null, and email address does not have a name, return the first portion of the address
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressAddress()
        {
            EventDO curEvent = _fixture.TestEvent2();
            UserDO curOriginator = curEvent.CreatedBy;
            curOriginator.FirstName = null;
            curOriginator.LastName = null;
            curOriginator.EmailAddress.Name = null;
            curOriginator.EmailAddress.Address = "john@smallberries.com";
            string originator = _comm.GetOriginatorName(curEvent);
            Assert.AreEqual(originator, "john");
        }

        //if first name and last name are null and email address has an invalid email address field, expect an exception
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_FailsIfNoNameData()
        {
            EventDO curEvent = _fixture.TestEvent2();
            UserDO curOriginator = curEvent.CreatedBy;
            curOriginator.FirstName = null;
            curOriginator.LastName = null;
            curOriginator.EmailAddress.Name = null;
            curOriginator.EmailAddress.Address = null;
            Assert.Throws<ValidationException>(() =>
            {
                string originator = _comm.GetOriginatorName(curEvent);
            });


        }
    }
}
