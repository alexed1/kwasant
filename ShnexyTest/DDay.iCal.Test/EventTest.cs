using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.ExtensionMethods;
using Data.DDay.DDay.iCal.Interfaces;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using NUnit.Framework;
using StructureMap;

namespace ShnexyTest.DDay.iCal.Test
{
    [TestFixture]
    public class EventTest
    {
        private string tzid;
        private IUnitOfWork _uow;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
        }
        
        /// <summary>
        /// Ensures that events can be properly added to a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Add1()
        {
            IICalendar iCal = new iCalendar();
            
            Event evt = new Event();
            evt.Summary = "Testing";
            evt.Start = new iCalDateTime(2010, 3, 25);
            evt.End = new iCalDateTime(2010, 3, 26);
            
            iCal.Events.Add(evt);
            Assert.AreEqual(1, iCal.Children.Count);
            Assert.AreSame(evt, iCal.Children[0]);            
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Remove1()
        {
            IICalendar iCal = new iCalendar();

            Event evt = new Event();
            evt.Summary = "Testing";
            evt.Start = new iCalDateTime(2010, 3, 25);
            evt.End = new iCalDateTime(2010, 3, 26);

            iCal.Events.Add(evt);
            Assert.AreEqual(1, iCal.Children.Count);
            Assert.AreSame(evt, iCal.Children[0]);

            iCal.RemoveChild(evt);
            Assert.AreEqual(0, iCal.Children.Count);
            Assert.AreEqual(0, iCal.Events.Count);
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Remove2()
        {
            IICalendar iCal = new iCalendar();

            Event evt = new Event();
            evt.Summary = "Testing";
            evt.Start = new iCalDateTime(2010, 3, 25);
            evt.End = new iCalDateTime(2010, 3, 26);

            iCal.Events.Add(evt);
            Assert.AreEqual(1, iCal.Children.Count);
            Assert.AreSame(evt, iCal.Children[0]);

            iCal.Events.Remove(evt);
            Assert.AreEqual(0, iCal.Children.Count);
            Assert.AreEqual(0, iCal.Events.Count);
        }
    }
}
