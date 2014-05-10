using System;
using System.Text;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Infrastructure;

using KwasantTest.Fixtures;
using KwasantCore.Services;
using KwasantCore.StructureMap;

using StructureMap;
using NUnit.Framework;

namespace KwasantTest.Models
{
    [TestFixture]
    public class CalendarDOTests
    {
        private IUnitOfWork _uow;
        private FixtureData _fixture;

        IEmailAddressRepository emailAddressRepo;
        IPersonRepository personRepo;
        ICalendarRepository calendarRepo;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            emailAddressRepo = new EmailAddressRepository(_uow);
            personRepo = new PersonRepository(_uow);
            calendarRepo = new CalendarRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        public CalendarDO SetupCalendarForTests()
        {
            PersonDO curPersonDO = _fixture.TestPerson1();
            personRepo.Add(curPersonDO);
            personRepo.UnitOfWork.SaveChanges();

            CalendarDO calendarDO = new CalendarDO()
            {
                Name = "Calendar Test",
                PersonId = curPersonDO.PersonId
            };

            return calendarDO;
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Create_CanCreateCalendar()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            //VERIFY
            CalendarDO curRetrievedCalendarDO = calendarRepo.GetByKey(curOriginalCalendarDO.CalendarId);
            Assert.AreEqual(curOriginalCalendarDO.CalendarId, curRetrievedCalendarDO.CalendarId);
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Create_FailsWithoutName()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();
            curOriginalCalendarDO.Name = null;            

            //EXECUTE
            calendarRepo.Create(curOriginalCalendarDO);
            var ex = Assert.Throws<Exception>(() =>
             {
                 calendarRepo.UnitOfWork.SaveChanges();
             }
             );

            //VERIFY
            Assert.AreNotEqual(ex.Message.IndexOf("Validation failed for one or more entities"), -1);
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Update_CanUpdateCalendar()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            String strCalenderName = "Calendar Test Updated";

            CalendarDO curUpdateCalendarDO = SetupCalendarForTests();
            curUpdateCalendarDO.Name = strCalenderName;

            CalendarDO curRetrievedCalendarDO = calendarRepo.GetByKey(curOriginalCalendarDO.CalendarId);

            calendarRepo.Update(curUpdateCalendarDO, curRetrievedCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            CalendarDO curUpdatedCalendarDO = calendarRepo.GetByKey(curRetrievedCalendarDO.CalendarId);

            //VERIFY
            Assert.AreEqual(curUpdatedCalendarDO.Name, strCalenderName);
            Assert.AreEqual(curUpdatedCalendarDO.PersonId, curUpdateCalendarDO.PersonId);
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Delete_CanDeleteCalendar()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            CalendarDO curRetrievedCalendarDO = calendarRepo.GetByKey(curOriginalCalendarDO.CalendarId);

            calendarRepo.Delete(curRetrievedCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            //VERIFY
            CalendarDO curDeletedCalendarDO = calendarRepo.GetByKey(curRetrievedCalendarDO.CalendarId);
            Assert.IsNull(curDeletedCalendarDO);
        }
    }
}
