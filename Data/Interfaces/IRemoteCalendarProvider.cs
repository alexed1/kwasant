using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.Entities.Enumerations;

namespace Data.Interfaces
{
    public interface IRemoteCalendarProvider
    {
        int Id { get; set; }
        string Name { get; set; }
        ServiceAuthorizationType AuthType { get; set; }
        string AppCreds { get; set; }
        string CalDAVEndPoint { get; set; }
    }
}