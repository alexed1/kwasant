using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.IdentityManager;
using KwasantWeb.ViewModels;
using StructureMap;
using AutoMapper;

namespace KwasantWeb.Controllers
{
    public class ClarificationResponseController : Controller
    {
        [KwasantAuthorize(Roles = "Customer")]
        [HttpPost]
        public ActionResult ProcessSubmittedClarificationData(ClarificationResponseViewModel responseViewModel)
        {
            // TODO: implement functionality
            return Json(new { success = true });
        }
	}
}