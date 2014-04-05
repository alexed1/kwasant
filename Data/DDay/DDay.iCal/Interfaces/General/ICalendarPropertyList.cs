using Data.DDay.Collections.Interfaces;
using Data.DDay.DDay.iCal.General;

namespace Data.DDay.DDay.iCal.Interfaces.General
{
    public interface ICalendarPropertyList :
        IGroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        ICalendarProperty this[string name] { get; }
    }
}
