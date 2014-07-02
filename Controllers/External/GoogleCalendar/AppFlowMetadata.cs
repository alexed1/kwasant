using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Utilities;

namespace KwasantWeb.Controllers.GoogleCalendar
{
    public class AppFlowMetadata : FlowMetadata
    {
        private readonly string _userId;
        private readonly IAuthorizationCodeFlow _flow;

        public AppFlowMetadata(string userId)
        {
            _userId = userId;
            _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                                                        {
                                                            ClientSecrets = new ClientSecrets
                                                                                {
                                                                                    ClientId = ConfigRepository.Get("GoogleCalendarClientId"),
                                                                                    ClientSecret = ConfigRepository.Get("GoogleCalendarClientSecret")
                                                                                },
                                                            Scopes = new[] { Google.Apis.Calendar.v3.CalendarService.Scope.Calendar },
                                                            DataStore = new EfDataStore(_userId)
                                                        });
        }

        public override string GetUserId(Controller controller)
        {
            return _userId;
        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return _flow; }
        }

        public override string AuthCallback
        {
            get
            {
                return "/AuthCallback/IndexAsync";
            }
        }
    }
}