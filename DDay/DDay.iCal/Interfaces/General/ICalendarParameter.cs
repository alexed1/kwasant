using System;
using System.Collections.Generic;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
