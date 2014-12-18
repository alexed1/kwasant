using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using StructureMap;
using Task = System.Threading.Tasks.Task;

namespace KwasantCore.Managers.APIManagers.Authorizers.Exchange
{
    public class ExchangeCalendarAuthorizer : IRemoteCalendarServiceAuthorizer
    {
        public async Task<IAuthorizationResult> GrantAccessAsync(string userId, string email, 
            string callbackUrl, string currentUrl, CancellationToken cancellationToken)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userAuthData = uow.RemoteCalendarAuthDataRepository.GetQuery().SingleOrDefault(d => d.Provider.Name == RemoteCalendarProviderDO.ExchangeProviderName && d.UserID == userId);
                if (userAuthData != null)
                {
                    return new AuthorizationResult(true, null);
                }
            }
            return new AuthorizationResult(false, new Uri(new Uri(currentUrl), string.Format("/ExchangeAuth/Login?returnUrl={0}", HttpUtility.UrlEncode(currentUrl))).ToString());
        }

        public async Task RevokeAccessAsync(string userId, CancellationToken cancellationToken)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userAuthData = uow.RemoteCalendarAuthDataRepository.GetQuery().SingleOrDefault(d => d.Provider.Name == RemoteCalendarProviderDO.ExchangeProviderName && d.UserID == userId);
                if (userAuthData != null)
                {
                    uow.RemoteCalendarAuthDataRepository.Remove(userAuthData);
                }
                uow.SaveChanges();
            }
        }

        public async Task AuthorizeAsync(string userId, string email, string password)
        {
            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(email, password);
            service.AutodiscoverUrl(email, ValidateRedirectionUrlCallback);
            // tries to get an item in the calendar folder to check user's credentials.
            var folders = service.FindFolders(WellKnownFolderName.Calendar, new FolderView(1));

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userAuthData = uow.RemoteCalendarAuthDataRepository.GetOrCreate(userId, RemoteCalendarProviderDO.ExchangeProviderName);
                var dataStore = new JSONDataStore(() => userAuthData.AuthData, s => userAuthData.AuthData = s);
                await dataStore.ClearAsync();
                await dataStore.StoreAsync("email", email);
                await dataStore.StoreAsync("password", password);
                uow.SaveChanges();
            }
        }

        private bool ValidateRedirectionUrlCallback(string redirectionUrl)
        {
            return true;
        }
    }
}
