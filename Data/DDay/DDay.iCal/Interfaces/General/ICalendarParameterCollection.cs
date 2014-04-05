using System;
using System.Collections.Generic;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarParameterCollection :
        IGroupedList<string, ICalendarParameter>
    {
        void SetParent(ICalendarObject parent);
        void Add(string name, string value);
        string Get(string name);
        IList<string> GetMany(string name);
        void Set(string name, string value);
        void Set(string name, IEnumerable<string> values);
    }
}
