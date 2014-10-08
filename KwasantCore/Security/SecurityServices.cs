using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Web;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
            var um = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db as DbContext));
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Sample");
            um.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<UserDO>(provider.Create("EmailConfirmation"));

            UserManager<UserDO> curUserManager = um;
            return curUserManager.CreateIdentity(userDO, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
