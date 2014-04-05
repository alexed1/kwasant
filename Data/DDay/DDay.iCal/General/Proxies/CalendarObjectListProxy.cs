using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;
using Shnexy.DDay.Collections.Proxies;

namespace Shnexy.DDay.iCal
{
    public class CalendarObjectListProxy<TType> :
        GroupedCollectionProxy<string, ICalendarObject, TType>,
        ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list)
        {
        }

        virtual public TType this[int index]
        {
            get
            {
                return this.Skip(index).FirstOrDefault();
            }
        }
    }
}
