using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Web;

namespace KwasantCore.Managers.APIManagers.Authorizers.Google
{
    public class GoogleCalendarAuthorizer : IOAuthAuthorizer
    {
        public FlowMetadata CreateFlowMetadata(string userId, string email = null, string callbackUrl = null)
        {
            return new AppFlowMetadata(userId, email, callbackUrl);
        }

        private IAuthorizationCodeFlow CreateFlow(string userId)
        {
            return CreateFlowMetadata(userId).Flow;
        }

        public async Task<AuthorizationCodeWebApp.AuthResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl, CancellationToken cancellationToken)
        {
            var flowMetadata = CreateFlowMetadata(userId, email, callbackUrl);
            return await new AuthorizationCodeWebApp(flowMetadata.Flow, flowMetadata.AuthCallback, currentUrl)
                             .AuthorizeAsync(userId, CancellationToken.None);
        }

        public async Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            await flow.RevokeTokenAsync(userId, tokenResponse.AccessToken, cancellationToken);
            await flow.DeleteTokenAsync(userId, cancellationToken);
        }

        public async Task RefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            if (tokenResponse == null)
                throw new UnauthorizedAccessException(string.Format("No refresh token found for user '{0}'.", userId));
            await flow.RefreshTokenAsync(userId, tokenResponse.RefreshToken, cancellationToken);
        }

        public async Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            if (tokenResponse == null)
                throw new UnauthorizedAccessException(string.Format("No access token found for user '{0}'.", userId));
            return tokenResponse.AccessToken;
        }
    }
}
