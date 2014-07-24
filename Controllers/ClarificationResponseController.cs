using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantCore.Services;
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
            var curClarificationRequestDO =
                Mapper.Map<ClarificationResponseViewModel, ClarificationRequestDO>(responseViewModel);
            try
            {
                var clarificationRequest = new ClarificationRequest();
                clarificationRequest.ProcessResponse(curClarificationRequestDO);
            }
            catch (EntityNotFoundException ex)
            {
                return HttpNotFound(ex.Message);
            }
            return View();
        }
	}
}