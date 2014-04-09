using System;
using Data.DDay.Collections;
using Data.DDay.DDay.iCal.Interfaces.General;

namespace Data.DDay.DDay.iCal.General
{
    /// <summary>
    /// A collection of calendar objects.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarObjectList :
        GroupedList<string, ICalendarObject>,
        ICalendarObjectList<ICalendarObject>
    {
        ICalendarObject _Parent;

        public CalendarObjectList(ICalendarObject parent)
        {
            _Parent = parent;
        }
    }
}
