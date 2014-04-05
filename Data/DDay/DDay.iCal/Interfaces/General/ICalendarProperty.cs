using Data.DDay.Collections.Interfaces;

namespace Data.DDay.DDay.iCal.Interfaces.General
{
    public interface ICalendarProperty :        
        ICalendarParameterCollectionContainer,
        ICalendarObject,
        IValueObject<object>
    {
        object Value { get; set; }
    }
}
