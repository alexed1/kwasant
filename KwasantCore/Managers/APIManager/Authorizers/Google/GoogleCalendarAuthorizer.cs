using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Web;
using Utilities;

namespace KwasantCore.Managers.APIManager.Authorizers.Google
{
    public class GoogleCalendarAuthorizer
    {
        class AuthorizationCodeFlow : GoogleAuthorizationCodeFlow
        {
            private readonly string _email;

            public AuthorizationCodeFlow(Initializer initializer, string email)
                : base(initializer)
            {
                _email = email;
            }

            public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
            {
                var url = base.CreateAuthorizationCodeRequest(redirectUri);
                var googleUrl = url as GoogleAuthorizationCodeRequestUrl;
                if (googleUrl != null)
                {
                    googleUrl.AccessType = "offline";
                    googleUrl.ApprovalPrompt = "force";
                    googleUrl.LoginHint = _email;
                }
                return url;
            }
        }

        private readonly AppFlowMetadata _flowMetadata;

        public GoogleCalendarAuthorizer(string userId, string email = null)
        {
            UserId = userId;

            _flowMetadata = new AppFlowMetadata(userId, email);
        }

        public string UserId { get; private set; }
        public FlowMetadata FlowMetadata { get { return _flowMetadata; } }
        public IAuthorizationCodeFlow Flow { get { return _flowMetadata.Flow; } }

        public async Task<AuthorizationCodeWebApp.AuthResult> AuthorizeAsync(string callbackUrl, string currentUrl, CancellationToken cancellationToken)
        {
            _flowMetadata.SetCallbackUrl(callbackUrl);
            return await new AuthorizationCodeWebApp(Flow, _flowMetadata.AuthCallback, currentUrl)
                             .AuthorizeAsync(UserId, CancellationToken.None);
        }

        public async Task RevokeAccessTokenAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await Flow.LoadTokenAsync(UserId, cancellationToken);
            await Flow.RevokeTokenAsync(UserId, tokenResponse.AccessToken, cancellationToken);
            await Flow.DeleteTokenAsync(UserId, cancellationToken);
        }

        public async Task RefreshTokenAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await Flow.LoadTokenAsync(UserId, cancellationToken);
            await Flow.RefreshTokenAsync(UserId, tokenResponse.RefreshToken, cancellationToken);
        }
    }
}
