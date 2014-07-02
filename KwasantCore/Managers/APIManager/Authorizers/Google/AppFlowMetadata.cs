using System.Web.Mvc;
using Data.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Utilities;

namespace KwasantCore.Managers.APIManager.Authorizers.Google
{
    class AppFlowMetadata : FlowMetadata
    {
        private readonly string _userId;
        private string _authCallbackUrl;
        private readonly IAuthorizationCodeFlow _flow;

        public AppFlowMetadata(string userId, string email = null)
        {
            _userId = userId;

            _flow = new AuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                                            {
                                                ClientId = ConfigRepository.Get("GoogleCalendarClientId"),
                                                ClientSecret = ConfigRepository.Get("GoogleCalendarClientSecret")
                                            },
                        Scopes = new[] {"https://www.googleapis.com/auth/calendar"},
                        DataStore = new JSONDataStore(_userId),
                    }, email);

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
            get { return _authCallbackUrl; }
        }

        public void SetCallbackUrl(string authCallbackUrl)
        {
            _authCallbackUrl = authCallbackUrl;
        }
    }
}