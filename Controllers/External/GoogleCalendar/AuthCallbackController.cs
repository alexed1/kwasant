using Google.Apis.Auth.OAuth2.Mvc;
using KwasantCore.Managers.APIManager.Authorizers.Google;
using KwasantCore.Managers.IdentityManager;

namespace KwasantWeb.Controllers.External.GoogleCalendar
{
    [KwasantAuthorize]
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override FlowMetadata FlowData
        {
            get { return new GoogleCalendarAuthorizer(this.GetUserId()).FlowMetadata; }
        }
    }
}