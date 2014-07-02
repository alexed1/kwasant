using Google.Apis.Auth.OAuth2.Mvc;
using KwasantCore.Managers.IdentityManager;

namespace KwasantWeb.Controllers.GoogleCalendar
{
    [KwasantAuthorize]
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(this.GetUserId()); }
        }
    }
}