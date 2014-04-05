using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shnexy.DDay.Collections.Interfaces.Proxies;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
