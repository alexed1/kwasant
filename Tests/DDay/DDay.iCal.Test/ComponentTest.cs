using Data.DDay.DDay.iCal;
using DDay.DDay.iCal.Components;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class ComponentTest
    {
        [Test, Category("Component")]
        public void UniqueComponent1()
        {
            iCalendar iCal = new iCalendar();
            DDayEvent evt = iCal.Create<DDayEvent>();

            Assert.IsNotNull(evt.UID);
            Assert.IsNull(evt.Created); // We don't want this to be set automatically
            Assert.IsNotNull(evt.DTStamp);
        }
    }
}
