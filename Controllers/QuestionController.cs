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


namespace KwasantWeb.Controllers
{
    public class QuestionController : Controller
    {
        //
        // GET: /Question/
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

	}
}