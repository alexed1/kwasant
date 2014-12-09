namespace KwasantCore.Managers.APIManagers.Authorizers
{
    class AuthorizationResult : IAuthorizationResult
    {
        public AuthorizationResult(bool isAuthorized, string redirectUri)
        {
            RedirectUri = redirectUri;
            IsAuthorized = isAuthorized;
        }

        public bool IsAuthorized { get; private set; }
        public string RedirectUri { get; private set; }
    }
}