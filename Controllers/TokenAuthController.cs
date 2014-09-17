using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Security;
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

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, validToken.UserDO);

                return Redirect(validToken.RedirectURL);
            }
        }
	}
}