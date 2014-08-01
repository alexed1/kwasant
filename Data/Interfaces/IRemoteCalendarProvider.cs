using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.Entities.Constants;

namespace Data.Interfaces
{
    public interface IRemoteCalendarProvider
    {
        int Id { get; set; }
        string Name { get; set; }
        ServiceAuthorizationTypeRow AuthType { get; set; }
        string AppCreds { get; set; }
        string CalDAVEndPoint { get; set; }
    }
}