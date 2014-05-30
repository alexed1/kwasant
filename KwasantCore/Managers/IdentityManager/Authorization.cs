using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KwasantCore.Managers.IdentityManager
{
    public class KwasantAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            // redirect to Error page
            context.Result = new RedirectResult("/index.aspx");
        }
    }
}
