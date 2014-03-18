using System;
using System.Collections.Generic;
using System.Text;

namespace Shnexy.DDay.iCal
{
    public interface IEncodableDataType :
        ICalendarDataType
    {
        string Encoding { get; set; }
    }
}
