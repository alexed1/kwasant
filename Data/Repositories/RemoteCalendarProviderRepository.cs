using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Newtonsoft.Json;
using Utilities;

namespace Data.Repositories
{
    public class RemoteCalendarProviderRepository : GenericRepository<RemoteCalendarProviderDO>, IRemoteCalendarProviderRepository
    {
        internal RemoteCalendarProviderRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteCalendarProviderDO GetByName(string name)
        {
            return GetQuery().FirstOrDefault(rcp => rcp.Name == name);
        }

        public void CreateRemoteCalendarProviders(IConfigRepository configRepository)
        {
            var googleClientId = configRepository.Get("GoogleCalendarClientId");
            var googleClientSecret = configRepository.Get("GoogleCalendarClientSecret");
            var microsoftClientId = configRepository.Get("MicrosoftCalendarClientId");
            var microsoftClientSecret = configRepository.Get("MicrosoftCalendarClientSecret");
            var providers = new[]
                {
                    new RemoteCalendarProviderDO
                        {
                            Name = "Google",
                            AuthType = ServiceAuthorizationType.OAuth2,
                            AppCreds = JsonConvert.SerializeObject(
                                new
                                    {
                                        ClientId = googleClientId,
                                        ClientSecret = googleClientSecret,
                                        Scopes = "https://www.googleapis.com/auth/calendar,email"
                                    }),
                            Interface = RemoteCalendarServiceInterface.CalDAV,
                            EndPoint = "https://apidata.googleusercontent.com/caldav/v2"
                        },
                    new RemoteCalendarProviderDO
                        {
                            Name = "Microsoft",
                            AuthType = ServiceAuthorizationType.OAuth2,
                            AppCreds = JsonConvert.SerializeObject(
                                new
                                    {
                                        ClientId = microsoftClientId,
                                        ClientSecret = microsoftClientSecret,
                                        Resource = "https://www.googleapis.com/auth/calendar,email"
                                    }),
                            Interface = RemoteCalendarServiceInterface.CalDAV,
                            EndPoint = "https://apidata.googleusercontent.com/caldav/v2"
                        },
                };
            foreach (var provider in providers)
            {
                var existingRow = GetByName(provider.Name);
                if (existingRow == null)
                {
                    Add(provider);
                }
                else
                {
                    existingRow.AuthType = provider.AuthType;
                    existingRow.AppCreds = provider.AppCreds;
                    existingRow.EndPoint = provider.EndPoint;
                }
            }
        }
    }

    public interface IRemoteCalendarProviderRepository : IGenericRepository<RemoteCalendarProviderDO>
    {
        RemoteCalendarProviderDO GetByName(string name);
    }
}
