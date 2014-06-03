using Data.Entities;

namespace Data.Interfaces
{
    public interface IEvent
    {
        int GenerateEmail (EventDO curEventDO);
        string GetOriginatorName(EventDO curEventDO);
    }
}