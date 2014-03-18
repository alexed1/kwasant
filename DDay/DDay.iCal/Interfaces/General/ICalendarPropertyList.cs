using System;
using System.Collections.Generic;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarPropertyList :
        IGroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        ICalendarProperty this[string name] { get; }
    }
}
