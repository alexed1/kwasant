using System.Web.Mvc;

namespace KwasantCore.Managers
{
    public class KwasantAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            // redirect to Error page
            context.Result = new RedirectResult("/Account/Index?returnUrl=" + context.RequestContext.HttpContext.Request.RawUrl);
        }
    }
}
