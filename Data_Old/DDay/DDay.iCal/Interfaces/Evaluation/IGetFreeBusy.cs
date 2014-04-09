using Data.DDay.DDay.iCal.Interfaces.Components;
using Data.DDay.DDay.iCal.Interfaces.DataTypes;

namespace Data.DDay.DDay.iCal.Interfaces.Evaluation
{
    public interface IGetFreeBusy
    {
        IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest);
        IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive);        
    }
}
