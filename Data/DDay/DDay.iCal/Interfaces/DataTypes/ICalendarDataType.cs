using System;
using Data.DDay.DDay.iCal.Interfaces.General;
using IServiceProvider = Data.DDay.DDay.iCal.Interfaces.General.IServiceProvider;

namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface ICalendarDataType :
        ICalendarParameterCollectionContainer,
        ICopyable,
        IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        IICalendar Calendar { get; }

        string Language { get; set; }
    }
}
