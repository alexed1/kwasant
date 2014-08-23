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
        public ActionResult ProcessSubmittedClarificationData(ClarificationResponseVM responseVM)
        {
            var curClarificationRequestDO =
                Mapper.Map<ClarificationResponseVM, ClarificationRequestDO>(responseVM);
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