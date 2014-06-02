using Data.Entities;

namespace Data.Interfaces
{
    public interface IEvent
    {
        void Dispatch(EventDO curEventDO);
        string GetOriginatorName(EventDO curEventDO);
    }
}