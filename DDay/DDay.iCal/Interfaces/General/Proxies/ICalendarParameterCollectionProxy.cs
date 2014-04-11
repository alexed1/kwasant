using Data.DDay.Collections.Interfaces.Proxies;

namespace Data.DDay.DDay.iCal.Interfaces.General.Proxies
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
