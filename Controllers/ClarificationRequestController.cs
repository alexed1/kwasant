using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KwasantCore.Managers.IdentityManager;
using KwasantWeb.ViewModels;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class ClarificationRequestController : Controller
    {
        public ActionResult Edit(int bookingRequestId)
        {
            return View(new ClarificationRequestViewModel());
        }
    }
}