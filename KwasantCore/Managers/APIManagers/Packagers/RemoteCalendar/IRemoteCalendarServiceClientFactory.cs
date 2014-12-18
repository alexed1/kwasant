using Data.Interfaces;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar
{
    public interface IRemoteCalendarServiceClientFactory
    {
        IRemoteCalendarServiceClient Create(IRemoteCalendarAuthDataDO authData);
    }
}
