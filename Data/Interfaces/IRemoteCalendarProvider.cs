using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IRemoteCalendarProvider
    {
        int Id { get; set; }
        string Name { get; set; }
        ServiceAuthorizationTypeDO AuthType { get; set; }
        string AppCreds { get; set; }
        string CalDAVEndPoint { get; set; }
    }
}