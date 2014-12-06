using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Web;

namespace KwasantCore.Managers.APIManagers.Authorizers
{
    public interface IOAuthAuthorizer : IRemoteCalendarServiceAuthorizer
    {
        Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken);
        Task RefreshTokenAsync(string userId, CancellationToken cancellationToken);
        Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken);
    }
}