using Data.Entities;

namespace Data.Interfaces
{
    public interface IEvent
    {
        int Dispatch(EventDO curEventDO);
        string GetOriginatorName(EventDO curEventDO);
    }
}