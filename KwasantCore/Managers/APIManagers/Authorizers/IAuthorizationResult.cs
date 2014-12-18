namespace KwasantCore.Managers.APIManagers.Authorizers
{
    public interface IAuthorizationResult
    {
        bool IsAuthorized { get; }
        string RedirectUri { get; }
    }
}