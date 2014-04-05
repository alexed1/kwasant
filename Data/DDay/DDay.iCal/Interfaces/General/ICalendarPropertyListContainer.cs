using System;
using System.Collections.Generic;
using System.Text;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarPropertyListContainer :
        ICalendarObject
    {
        ICalendarPropertyList Properties { get; }
    }
}
