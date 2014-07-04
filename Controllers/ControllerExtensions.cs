using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace KwasantWeb.Controllers
{
    public static class ControllerExtensions
    {
        public static string GetUserId(this Controller controller)
        {
            return controller.User.Identity.GetUserId();
        }
    }
}