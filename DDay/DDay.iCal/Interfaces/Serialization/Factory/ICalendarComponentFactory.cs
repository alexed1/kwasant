using Data.DDay.DDay.iCal.Interfaces.Components;

namespace Data.DDay.DDay.iCal.Interfaces.Serialization.Factory
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName, bool uninitialized);
    }
}
