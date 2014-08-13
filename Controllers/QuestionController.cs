using System.Linq;
using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Services;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using System.Collections.Generic;


namespace KwasantWeb.Controllers
{
    public class QuestionController : Controller
    {

        //
        // GET: /Question/
      
        private Question _question;
        public QuestionController()
        {
            _question = new Question();
        }

        public ActionResult EditTimeslots(int? calendarID, int? negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                CalendarDO calendarDO = null;
                if (calendarID != null)
                    calendarDO = uow.CalendarRepository.FindOne(c => c.Id == calendarID);

                if (calendarDO == null)
                {
                    //The rest of the calendar will be updated later
                    calendarDO = new CalendarDO
                    {
                        Name = "Negotiation Caledar",
                        NegotiationID = negotiationID,
                        OwnerID = User.Identity.GetUserId()
                    };
                    uow.CalendarRepository.Add(calendarDO);
                    uow.SaveChanges();
                }
                return Json(calendarDO.Id, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public PartialViewResult Create(int questionId, int negotiationId = 0)
        {
            List<int> quesId= new List<int>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                quesId = _question.CreateQuestion(uow, questionId, negotiationId);
            }
            return PartialView("~/Views/Negotiation/_Question.cshtml", quesId);
        }

        public JsonResult Delete(int questionId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _question.DeleteQuestion(uow, questionId);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
  
    }
}