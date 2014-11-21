using Data.States.Templates;

namespace Data.Interfaces
{
    public interface IRemoteCalendarProviderDO : IBaseDO
    {
        int Id { get; set; }
        string Name { get; set; }
        string AppCreds { get; set; }
        string EndPoint { get; set; }
        int? Interface { get; set; }
        int? AuthType { get; set; }
    }
}