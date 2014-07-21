using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Web;

namespace KwasantCore.Managers.APIManager.Authorizers
{
    public interface IOAuthAuthorizer
    {
        Task<AuthorizationCodeWebApp.AuthResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl, CancellationToken cancellationToken);
        Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken);
        Task RefreshTokenAsync(string userId, CancellationToken cancellationToken);
        Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken);
    }
}