using System;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.CalDAV.Google;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public class CalDAVClientFactory : ICalDAVClientFactory
    {
        #region Implementation of ICalDAVClientFactory

        public ICalDAVClient Create(IUser user, string provider)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            GoogleCalDAVEndPoint endPoint;
            GoogleOAuthHttpChannel channel;
            switch (provider)
            {
                case "Google" :
                    endPoint = new GoogleCalDAVEndPoint();
                    channel = new GoogleOAuthHttpChannel(user.Id);
                    break;
                default :
                    throw new ArgumentOutOfRangeException("provider");
            }
            return new CalDAVClient(endPoint, channel);
        }

        #endregion
    }
}