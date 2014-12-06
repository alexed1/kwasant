using System;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar.CalDAV;
using KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar.EWS;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using StructureMap;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar
{
    public class RemoteCalendarServiceClientFactory : IRemoteCalendarServiceClientFactory
    {
        public IRemoteCalendarServiceClient Create(IRemoteCalendarAuthDataDO authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");

            IAuthorizingHttpChannel channel;
            switch (authData.Provider.AuthType)
            {
                case ServiceAuthorizationType.OAuth2 : 
                    channel = new OAuthHttpChannel(ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(authData.Provider.Name));
                    break;
                default :
                    throw new NotSupportedException(string.Format("Unknown calendar provider ('{0}') auth type: {1}", 
                        authData.Provider.Name, authData.Provider.AuthType));
            }

            // provider specific switch
            if (string.Equals(authData.Provider.Name, "google", StringComparison.OrdinalIgnoreCase))
            {
                return new GoogleCalDAVClient(authData.Provider.EndPoint, channel);
            }
/*
            else if (string.Equals(authData.Provider.Name, "exchange", StringComparison.OrdinalIgnoreCase))
            {
                // return Exchange specific EWS client if needed
            }
*/
            // interface specific switch
            switch (authData.Provider.Interface)
            {
                case RemoteCalendarServiceInterface.CalDAV:
                    return new CalDAVClient(authData.Provider.EndPoint, channel);
                case RemoteCalendarServiceInterface.EWS:
                    return new EWSClient();
            }

            throw new NotSupportedException(string.Format("Unknown calendar provider ('{0}') interface: {1}",
                                                          authData.Provider.Name, authData.Provider.Interface));
        }
    }
}