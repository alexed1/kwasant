using System;
using Data.Entities;
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

            // provider specific switch
            if (string.Equals(authData.Provider.Name, RemoteCalendarProviderDO.GoogleProviderName, StringComparison.OrdinalIgnoreCase))
            {
                return new GoogleCalDAVClient(authData.Provider.EndPoint, CreateChannel(authData.Provider));
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
                    return new CalDAVClient(authData.Provider.EndPoint, CreateChannel(authData.Provider));
                case RemoteCalendarServiceInterface.EWS:
                    return new EWSClient(authData);
            }

            throw new NotSupportedException(string.Format("Unknown calendar provider ('{0}') interface: {1}",
                                                          authData.Provider.Name, authData.Provider.Interface));
        }

        private IAuthorizingHttpChannel CreateChannel(IRemoteCalendarProviderDO provider)
        {
            switch (provider.AuthType)
            {
                case ServiceAuthorizationType.OAuth2:
                    return new OAuthHttpChannel(ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(provider.Name));
                default:
                    throw new NotSupportedException(string.Format("Unknown calendar provider ('{0}') auth type: {1}",
                        provider.Name, provider.AuthType));
            }
        }
    }
}