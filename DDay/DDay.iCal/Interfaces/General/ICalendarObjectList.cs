using Data.DDay.Collections.Interfaces;

namespace Data.DDay.DDay.iCal.Interfaces.General
{    
    public interface ICalendarObjectList<TType> : 
        IGroupedCollection<string, TType>
        where TType : class, ICalendarObject
    {
        TType this[int index] { get; }
    }
}
