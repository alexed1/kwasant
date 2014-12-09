using System;
using Data.Entities;
using Data.Interfaces;
using FluentValidation;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class EventServiceTests : BaseTest
    {

        [Test]
        [Category("Event")]
        public void CanGetOrignatorNameWithValidEmailAddress()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();

                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreNotEqual(originator, null);
            }
        }

        //if first name and last name are null, but email address has a name, return the name
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressName()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = "John Smallberries";
                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreEqual(originator, curOriginator.EmailAddress.Name);
            }
        }

        //if first name and last name are null, and email address does not have a name, return the first portion of the address
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressAddress()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = null;
                curOriginator.EmailAddress.Address = "john@smallberries.com";
                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreEqual(originator, "john");
            }
        }

        //if first name and last name are null and email address has an invalid email address field, expect an exception
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_FailsIfNoNameData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = null;
                curOriginator.EmailAddress.Address = null;

                var invitation = ObjectFactory.GetInstance<Invitation>();
                Assert.Throws<ValidationException>(() =>
                {
                    string originator = invitation.GetOriginatorName(curEvent);
                });
            }
        }

        [Test]
        public void TestEventAtMidnightKeepsTimeComponent()
        {
            var evDO = new EventDO();
            evDO.CreatedBy = new UserDO { EmailAddress = new EmailAddressDO { Name = "Alex Edelstein" } };

            evDO.Description = @"Meeting with Paul Maeder, Campaign co-chair for the School of  Engineering and Applied Sciences.";
            evDO.Attendees.Add(new AttendeeDO { Name = "Alex Edelstein", EmailAddress = new EmailAddressDO("alex@edelstein.org") });
            evDO.Attendees.Add(new AttendeeDO { Name = "Dieterich, Joshua Ethan", EmailAddress = new EmailAddressDO("joshua_dieterich@harvard.edu") });
            evDO.Attendees.Add(new AttendeeDO { Name = "Outbound Archive", EmailAddress = new EmailAddressDO("kwasantoutbound@gmail.com") });
            evDO.Attendees.Add(new AttendeeDO { Name = "'Alexed15@gmail.com'", EmailAddress = new EmailAddressDO("alexed15@gmail.com") });
            evDO.StartDate = new DateTimeOffset(2014, 12, 09, 23, 0, 0, 0, new TimeSpan(0));
            evDO.EndDate = evDO.StartDate.AddHours(1);
            evDO.Location = "Harvard";
            evDO.Summary = "Harvard Meeting with Paul Maeder";

            var cal = Event.GenerateICSCalendarStructure(evDO);

            iCalendarSerializer serializer = new iCalendarSerializer(cal);
            string fileToAttach = serializer.Serialize(cal);

            Assert.True(fileToAttach.Contains("DTEND:20141210T000000Z"));
        }
    }
}
