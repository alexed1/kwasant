using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using KwasantCore.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KwasantWeb.Controllers
{
    public class DataController : Controller
    {
        EmailAddress _emailAddress;
        EmailAddressValidator _emailAddressValidator;

        public DataController()
        {
            _emailAddress = new EmailAddress();
            _emailAddressValidator = new EmailAddressValidator();
        }

        public ActionResult ValidateEmailAddress(string emailString)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailAddressDO emailAddressDO = _emailAddress.ConvertFromString(HttpUtility.UrlDecode(emailString), uow);
                    if (!(_emailAddressValidator.Validate(emailAddressDO).IsValid))
                        return Json("Invalid email format", JsonRequestBehavior.AllowGet);
                    else
                        return Json(true, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}