using Data.DDay.Collections.Interfaces;

namespace Data.DDay.DDay.iCal.Interfaces.General
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
