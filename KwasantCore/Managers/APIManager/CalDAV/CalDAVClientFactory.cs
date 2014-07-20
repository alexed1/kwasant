using System;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Authorizers;
using StructureMap;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public class CalDAVClientFactory : ICalDAVClientFactory
    {
        #region Implementation of ICalDAVClientFactory

        public ICalDAVClient Create(IRemoteCalendarAuthData authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");

            var channel = new OAuthHttpChannel(ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(authData.Provider.Name));
            return new CalDAVClient(authData.Provider.CalDAVEndPoint, channel);
        }

        #endregion
    }
}