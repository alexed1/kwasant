using System;
using System.Security.Claims;
using System.Web;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using StructureMap;

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
            ObjectFactory.GetInstance<ITracker>().Identify(userDO);
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
