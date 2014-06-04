using Data.Entities;

namespace Data.Interfaces
{
    public interface IEvent
    {
        void Dispatch(IUnitOfWork uow, EventDO curEventDO);
        string GetOriginatorName(EventDO curEventDO);
    }
}