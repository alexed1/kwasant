using System;
using KwasantCore.Managers.APIManager.Authorizers;
using KwasantCore.Managers.APIManager.Authorizers.Google;

namespace KwasantCore.Managers.APIManager.CalDAV.Google
{
    internal class GoogleOAuthHttpChannel : OAuthHttpChannelBase
    {
        private readonly GoogleCalendarAuthorizer _authorizer;

        public GoogleOAuthHttpChannel(string userId)
        {
            _authorizer = new GoogleCalendarAuthorizer(userId);
        }

        #region Overrides of OAuthHttpChannelBase

        public override IOAuthAuthorizer Authorizer
        {
            get { return _authorizer; }
        }

        #endregion
    }
}