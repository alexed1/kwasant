using Data.Interfaces;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public class CalDAVClientFactory : ICalDAVClientFactory
    {
        #region Implementation of ICalDAVClientFactory

        public ICalDAVClient Create(IUser user)
        {
            if (user.GrantedAccessToGoogleCalendar)
            {
                return new GoogleCalDAVClient(user.Id);
            }
            return null;
        }

        #endregion
    }
}