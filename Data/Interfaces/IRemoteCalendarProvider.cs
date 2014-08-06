using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.States.Templates;

namespace Data.Interfaces
{
    public interface IRemoteCalendarProvider
    {
        int Id { get; set; }
        string Name { get; set; }
        _ServiceAuthorizationTypeTemplate AuthTypeTemplate { get; set; }
        string AppCreds { get; set; }
        string CalDAVEndPoint { get; set; }
    }
}