using System.Collections.Generic;
using Data.DDay.DDay.iCal.Interfaces.DataTypes;
using Data.DDay.DDay.iCal.Interfaces.General;

namespace Data.DDay.DDay.iCal.Interfaces.Components
{
    public interface IFreeBusy :
        IUniqueComponent,
        IMergeable
    {
        IList<IFreeBusyEntry> Entries { get; set; }
        IDateTime DTStart { get; set; }
        IDateTime DTEnd { get; set; }
        IDateTime Start { get; set; }
        IDateTime End { get; set; }

        FreeBusyStatus GetFreeBusyStatus(IPeriod period);
        FreeBusyStatus GetFreeBusyStatus(IDateTime dt);
    }
}
