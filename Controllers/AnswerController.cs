using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;
using System.Collections.Generic;


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