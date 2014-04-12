using System.Linq;
using Data.DDay.Collections.Interfaces;
using Data.DDay.Collections.Proxies;
using Data.DDay.DDay.iCal.Interfaces.General;

namespace Data.DDay.DDay.iCal.General.Proxies
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
