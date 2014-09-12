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
        readonly EmailAddress _emailAddress;
        EmailAddressValidator _emailAddressValidator;

        public DataController()
        {
            _emailAddress = ObjectFactory.GetInstance<EmailAddress>();
            _emailAddressValidator = new EmailAddressValidator();
        }

        public ActionResult ValidateEmailAddress(string emailString)
        {
            if (HttpUtility.UrlDecode(emailString).IsEmailAddress())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Invalid email format", JsonRequestBehavior.AllowGet);
            }
        }

    }
}