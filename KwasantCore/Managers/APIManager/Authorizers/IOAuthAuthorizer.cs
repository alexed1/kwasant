using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Web;

namespace KwasantCore.Managers.APIManager.Authorizers
{
    public interface IOAuthAuthorizer
    {
        Task<AuthorizationCodeWebApp.AuthResult> AuthorizeAsync(string callbackUrl, string currentUrl, CancellationToken cancellationToken);
        Task RevokeAccessTokenAsync(CancellationToken cancellationToken);
        Task RefreshTokenAsync(CancellationToken cancellationToken);
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    }
}