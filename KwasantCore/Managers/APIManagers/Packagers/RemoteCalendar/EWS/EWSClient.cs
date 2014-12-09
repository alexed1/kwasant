using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantICS.DDay.iCal;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar.EWS
{
    class EWSClient : IRemoteCalendarServiceClient
    {
        private readonly IRemoteCalendarAuthDataDO _authData;
        private readonly Lazy<ExchangeService> _lazyExchangeService;

        public EWSClient(IRemoteCalendarAuthDataDO authData)
        {
            _authData = authData;
            _lazyExchangeService = new Lazy<ExchangeService>(CreateServiceClient);
        }

        private ExchangeService CreateServiceClient()
        {
            var serviceClient = new ExchangeService();
            var jsonStore = new JSONDataStore(() => _authData.AuthData, s => _authData.AuthData = s);
            var email = jsonStore.GetAsync<string>("email").Result;
            var password = jsonStore.GetAsync<string>("password").Result;
            serviceClient.Credentials = new WebCredentials(email, password);
            serviceClient.AutodiscoverUrl(email);
            return serviceClient;
        }

        private ExchangeService ExchangeService { get { return _lazyExchangeService.Value; } }

        public async Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to)
        {
            throw new NotImplementedException();
        }

        public async Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, iCalendar calendarEvent)
        {
            throw new NotImplementedException();
        }

        public async Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData)
        {
            return ExchangeService.FindFolders(WellKnownFolderName.Calendar, new FolderView(1))
                .ToDictionary(f => f.Id.UniqueId, f => f.DisplayName);
        }
    }
}
