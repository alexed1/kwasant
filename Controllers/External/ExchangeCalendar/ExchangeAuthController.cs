using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers.Exchange;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers.External.ExchangeCalendar
{
    [KwasantAuthorize]
    public class ExchangeAuthController : Controller
    {
        //
        // GET: /ExchangeAuth/
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                throw new ArgumentNullException("returnUrl");
            ViewBag.returnUrl = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginAsync(LoginVM loginVm, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var authorizer = ObjectFactory.GetInstance<ExchangeCalendarAuthorizer>();
                try
                {
                    await authorizer.AuthorizeAsync(this.GetUserId(), loginVm.Email, loginVm.Password);
                    return Redirect(returnUrl);
                }
                catch (Exception)
                {
                    // TODO: logging
                    throw;
                }
            }
            return View(loginVm);
        }
	}
}