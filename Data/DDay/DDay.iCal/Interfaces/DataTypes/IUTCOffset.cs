using System;

namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface IUTCOffset :
        IEncodableDataType
    {
        bool Positive { get; set; }
        int Hours { get; set; }
        int Minutes { get; set; }
        int Seconds { get; set; }

        DateTime ToUTC(DateTime dt);
        DateTime ToLocal(DateTime dt);
    }
}
