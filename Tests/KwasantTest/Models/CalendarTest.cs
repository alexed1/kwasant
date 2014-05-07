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
    public class CalendarTest
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

        public  CalendarDO SetupCalendarForTests()
        {
            PersonDO personDO = _fixture.TestPerson1();
            personRepo.Add(personDO);
            personRepo.UnitOfWork.SaveChanges();

            CalendarDO calendarDO = new CalendarDO()
            {
                Name = "Calendar Test",
                PersonId = personDO.PersonId
            };

            return calendarDO;
        }       

        [Test]
        [Category("Calendar")]
        public void Calendar_Create_CanCreateCalendar()
        {
            //SETUP      
            CalendarDO originalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(originalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            //VERIFY
            CalendarDO retrievedCalendarDO = calendarRepo.GetByKey(originalCalendarDO.CalendarId);
            Assert.AreEqual(originalCalendarDO.CalendarId, retrievedCalendarDO.CalendarId);
        }

        [Test]
        [Category("Calendar")]
        public void Calendar_Create_FailsWithoutName()
        {
            String strErrorMessage = String.Empty;

            //SETUP      
            CalendarDO originalCalendarDO = SetupCalendarForTests();
            originalCalendarDO.Name = null;            

            //EXECUTE
            try
            {
                calendarRepo.Create(originalCalendarDO);
                calendarRepo.UnitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
            }

            //VERIFY
            Assert.AreNotEqual(strErrorMessage.IndexOf("Validation failed for one or more entities"), -1);
        }

        [Test]
        [Category("Calendar")]
        public void Calendar_Read_CanReadCalendar()
        {
            //SETUP      
            CalendarDO originalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(originalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();           

            //VERIFY
            CalendarDO retrievedCalendarDO = calendarRepo.GetByKey(originalCalendarDO.CalendarId);
            Assert.AreEqual(originalCalendarDO.CalendarId, retrievedCalendarDO.CalendarId);
        }

        [Test]
        [Category("Calendar")]
        public void Calendar_Delete_CanDeleteCalendar()
        {
            //SETUP      
            CalendarDO originalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Create(originalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();           

            CalendarDO retrievedCalendarDO = calendarRepo.GetByKey(originalCalendarDO.CalendarId);

            calendarRepo.Delete(retrievedCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();                         

            //VERIFY
            CalendarDO deletedCalendarDO = calendarRepo.GetByKey(retrievedCalendarDO.CalendarId);
            Assert.IsNull(deletedCalendarDO);            
        }
    }
}
