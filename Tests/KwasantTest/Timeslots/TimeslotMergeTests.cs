using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc.Json;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Timeslots
{
    [TestFixture]
    public class TimeslotMergeTests
    {
        private UserDO _user;
        private const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss.fffffff \z";

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _user = new FixtureData().TestUser1();
        }

        private String GetFormattedDateString(String date)
        {
            return DateTime.Parse(date).ToString(DateStandardFormat);
        }

        private DateTime GetFormattedDate(String date)
        {
            return DateTime.ParseExact(GetFormattedDateString(date), DateStandardFormat, CultureInfo.InvariantCulture).ToUniversalTime();
        }
        
        private void CreateOriginalTimeSlot(int calendarID, String startTime, String endTime, bool nextDay = false)
        {
            var originalTimeSlot = new EventDO();
            originalTimeSlot.CreatedByID = "NOONE";

            originalTimeSlot.CalendarID = calendarID;
            originalTimeSlot.StartDate = GetFormattedDate(startTime);
            if (nextDay)
                originalTimeSlot.StartDate = originalTimeSlot.StartDate.AddDays(1);
            originalTimeSlot.EndDate = GetFormattedDate(endTime);
            if (nextDay)
                originalTimeSlot.EndDate = originalTimeSlot.EndDate.AddDays(1);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EventRepository.Add(originalTimeSlot);
                var calendarDO = uow.CalendarRepository.GetQuery().FirstOrDefault(c => c.Id == calendarID);

                if (calendarDO == null)
                {
                    calendarDO = CreateCalendarDO(uow, calendarID);
                }
                originalTimeSlot.Calendar = calendarDO;
                
                uow.SaveChanges();
            }
        }

        private CalendarDO CreateCalendarDO(IUnitOfWork uow, int calendarID)
        {
            
            var calendarDO = new CalendarDO()
            {
                Id = calendarID,
                Name = "Test Calendar",
                OwnerID = _user.Id,
                Owner = _user
            };
            uow.CalendarRepository.Add(calendarDO);
            return calendarDO;
        }

        private void SubmitNewTimeSlotToController(int EventID, String startTime, String endTime)
        {
            var controller = new EventController();

            var res = controller.NewTimeSlot(EventID, GetFormattedDateString(startTime), GetFormattedDateString(endTime), true);
            var converted = res as JavaScriptResult;
            if (converted == null)
                throw new Exception("Invalid return type");

            if (converted.Script != SimpleJsonSerializer.Serialize(true))
                throw new Exception("Error processing on server. Returned response: '" + converted.Script + "'");
        }

        [Test]
        public void TestDisjointedJoinedBeforeDoesNotMerge()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "10:30am", "11:00am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "11:30am", "11:45am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(2, all.Count());

                Assert.AreEqual(GetFormattedDate("10:30am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:00am"), all[0].EndDate.DateTime);

                Assert.AreEqual(GetFormattedDate("11:30am"), all[1].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:45am"), all[1].EndDate.DateTime);
            }
        }

        [Test]
        public void TestJointedJoinedBeforeDoesMerge()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "10:30am", "11:00am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "11:00am", "11:45am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("10:30am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:45am"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestJointedJoinedAfterDoesMerge()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "11:45am", "12:00pm");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "11:00am", "11:45am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("11:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("12:00pm"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestOverLapBeforeDoesMerge()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "9:00am", "11:00am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "10:00am", "11:30am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("9:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:30am"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestOverLapAfterDoesMerge()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "11:00am", "12:00pm");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "10:00am", "11:30am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("10:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("12:00pm"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestExistingFullyContainedInNewMerges()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "10:00am", "11:00am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "9:00am", "11:30am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("9:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:30am"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestNewFullyContainedInExistingMerges()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateOriginalTimeSlot(1, "9:00am", "11:30am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(1, "10:00am", "11:00am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(1, all.Count());

                Assert.AreEqual(GetFormattedDate("9:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:30am"), all[0].EndDate.DateTime);
            }
        }

        [Test]
        public void TestMergeOnlyWithinGroup()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateCalendarDO(uow, 2);

                CreateOriginalTimeSlot(1, "10:00am", "11:00am");

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(2, "9:00am", "11:30am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(2, all.Count());

                Assert.AreEqual(GetFormattedDate("10:00am"), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:00am"), all[0].EndDate.DateTime);

                Assert.AreEqual(GetFormattedDate("9:00am"), all[1].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:30am"), all[1].EndDate.DateTime);
            }
        }

        [Test]
        public void TestMergeRecognisesDifferentDates()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateCalendarDO(uow, 2);

                //This creates the event tomorrow at 10am
                CreateOriginalTimeSlot(1, "10:00am", "11:00am", true);

                Assert.AreEqual(1, uow.EventRepository.GetAll().Count());
                SubmitNewTimeSlotToController(2, "9:00am", "11:30am");

                var all = uow.EventRepository.GetAll().ToList();
                Assert.AreEqual(2, all.Count());

                Assert.AreEqual(GetFormattedDate("10:00am").AddDays(1), all[0].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:00am").AddDays(1), all[0].EndDate.DateTime);

                Assert.AreEqual(GetFormattedDate("9:00am"), all[1].StartDate.DateTime);
                Assert.AreEqual(GetFormattedDate("11:30am"), all[1].EndDate.DateTime);
            }
        }
    }
}
