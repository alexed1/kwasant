using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.Controllers.External.DayPilot;
using KwasantWeb.Controllers.External.DayPilot.Providers;
using KwasantWeb.ViewModels;
using StructureMap;
using Data.Entities;
using System.Collections.Generic;
using Data.Constants;


namespace KwasantWeb.Controllers
{
    public class AnswerController : Controller
    {
        //
        // GET: /Answer/
        #region Answer
        private Answer _answer;
        public AnswerController()
        {
            _answer = new Answer();
        }

        [HttpGet]
        public PartialViewResult Create(int answerId, int bookingRequestId, int questionId = 0)
        {
            List<int> ansId = new List<int>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ansId = _answer.CreateAnswer(uow, answerId, questionId, bookingRequestId);
            }
            return PartialView("~/Views/Negotiation/_TextAnswer.cshtml", ansId);
        }

        public JsonResult Delete(int answerId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _answer.DeleteAnswer(uow, answerId);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}