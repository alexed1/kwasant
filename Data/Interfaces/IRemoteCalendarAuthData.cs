namespace Data.Interfaces
{
    public interface IRemoteCalendarAuthData
    {
        int Id { get; set; }
        
        int? ProviderID { get; set; }
        IRemoteCalendarProvider Provider { get; set; }

        string UserID { get; set; }
        IUser User { get; set; }
        
        string AuthData { get; set; }
        bool HasAccessToken();
    }
}