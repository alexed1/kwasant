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
    public class QuestionController : Controller
    {

        //
        // GET: /Question/
        #region Question
        private Question _question;
        public QuestionController()
        {
            _question = new Question();
        }

        public ActionResult EditTimeslots(int Id, int BookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == Id);
                if (calendarDO == null)
                {
                    calendarDO = new CalendarDO();
                    calendarDO.Name = "Negotiation Caledar";
                    calendarDO.OwnerID = userDO.Id;
                    calendarDO.QuestionId = Id;
                    calendarDO.BookingRequests = uow.BookingRequestRepository.GetAll().Where(e => e.Id == BookingRequestID).ToList();
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
        #endregion
    }
}