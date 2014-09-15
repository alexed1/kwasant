using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class TokenAuthController : Controller
    {
        public ActionResult Index(String token)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.GetQuery().FirstOrDefault(t => t.Id.ToString() == token);
                if (validToken == null)
                    throw new HttpException(404, "Authorization token not found.");
                if (validToken.ExpiresAt < DateTime.Now)
                    throw new HttpException(404, "Authorization token expired.");

                UserManager<UserDO> curUserManager = KwasantCore.Services.User.GetUserManager(uow); ;
                ClaimsIdentity identity = curUserManager.CreateIdentity(validToken.UserDO, DefaultAuthenticationTypes.ApplicationCookie);

                System.Web.HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true
                }, identity);

                return View((object)validToken.RedirectURL);
            }
        }

        
	}
}