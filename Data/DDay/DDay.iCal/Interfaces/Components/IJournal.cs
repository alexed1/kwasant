using System;
using System.Collections.Generic;
using System.Text;

namespace Shnexy.DDay.iCal
{
    public interface IJournal :
        IRecurringComponent
    {
        JournalStatus Status { get; set; }
    }
}
