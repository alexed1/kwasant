using System;

namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface ITrigger :
        IEncodableDataType
    {
        IDateTime DateTime { get; set; }
        TimeSpan? Duration { get; set; }
        TriggerRelation Related { get; set; }
        bool IsRelative { get; }
    }
}
