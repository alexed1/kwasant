using System;
using System.Collections.Generic;
using System.Text;

namespace Shnexy.DDay.iCal
{
    public interface IStatusCode :
        IEncodableDataType
    {
        int[] Parts { get; set; }
        int Primary { get; }
        int Secondary { get; }
        int Tertiary { get; }
    }
}
