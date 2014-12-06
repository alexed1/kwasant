using System.Threading;
using System.Threading.Tasks;

namespace KwasantCore.Managers.APIManagers.Authorizers
{
    public interface IRemoteCalendarServiceAuthorizer
    {
        Task<IAuthorizationResult> GrantAccessAsync(string userId, string email, string callbackUrl, string currentUrl, CancellationToken cancellationToken);
        Task RevokeAccessAsync(string userId, CancellationToken cancellationToken);
    }
}