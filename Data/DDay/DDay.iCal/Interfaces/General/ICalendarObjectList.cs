using System;
using System.Collections.Generic;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;

namespace Shnexy.DDay.iCal
{    
    public interface ICalendarObjectList<TType> : 
        IGroupedCollection<string, TType>
        where TType : class, ICalendarObject
    {
        TType this[int index] { get; }
    }
}
