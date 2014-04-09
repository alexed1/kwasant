using System.Collections.Generic;
using Data.DDay.DDay.iCal.Interfaces.Evaluation;

namespace Data.DDay.DDay.iCal.Interfaces
{
    public interface IICalendarCollection :
        IGetOccurrencesTyped,
        IList<IICalendar>
    {        
    }
}
