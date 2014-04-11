using System;
using Data.DDay.DDay.iCal.Interfaces.DataTypes;
using Data.DDay.DDay.iCal.Interfaces.General;
using Data.DDay.DDay.iCal.Structs;

namespace Data.DDay.DDay.iCal.Interfaces.Components
{
    public interface ITimeZone :
        ICalendarComponent
    {
        string ID { get; set; }
        string TZID { get; set; }
        IDateTime LastModified { get; set; }
        Uri TZUrl { get; set; }
        Uri Url { get; set; }
        ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos { get; set; }
        TimeZoneObservance? GetTimeZoneObservance(IDateTime dt);
    }
}
