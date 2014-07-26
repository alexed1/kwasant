using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IRemoteCalendarAuthData
    {
        int Id { get; set; }
        
        int ProviderID { get; set; }
        IRemoteCalendarProvider Provider { get; set; }

        string UserID { get; set; }
        IUser User { get; set; }
        
        string AuthData { get; set; }
    }
}