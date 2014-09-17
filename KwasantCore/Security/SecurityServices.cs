using System;
using System.Security.Claims;
using System.Web;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace KwasantCore.Security
{
    class SecurityServices : ISecurityServices
    {
        public void Login(IUnitOfWork uow, UserDO userDO)
        {
            ClaimsIdentity identity = GetIdentity(uow, userDO);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
            {
                IsPersistent = true
            }, identity);
        }

        public String GetCurrentUser()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public String GetUserName()
        {
            return HttpContext.Current.User.Identity.GetUserName();
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public void Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut();
        }

        public ClaimsIdentity GetIdentity(IUnitOfWork uow, UserDO userDO)
        {
            UserManager<UserDO> curUserManager = Services.User.GetUserManager(uow); ;
            return curUserManager.CreateIdentity(userDO, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
