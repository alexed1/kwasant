using System;
using System.Net;
using System.Web.Mvc;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.Models;
using Data.DataAccessLayer.Repositories;
using DayPilot.Web.Mvc.Json;
using Shnexy.Controllers.Data;
using Shnexy.Controllers.DayPilot;
using StructureMap;

namespace Shnexy.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {
        #region "Action"

        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            PopulateCalender();

            var emailRepository = new EmailRepository(ObjectFactory.GetInstance<IUnitOfWork>());
            var email = emailRepository.GetByKey(id);
            if (email != null)
            {
                EventManager = new EventManager(this, email.From.Address);
                return View(email);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        private EventManager EventManager
        {
            get
            {
                return Session["EventManager"] as EventManager;
            }
            set
            {
                Session["EventManager"] = value;
            }
        }

        public ActionResult Rtl()
        {
            return View();
        }
        public ActionResult Columns50()
        {
            return View();
        }
        public ActionResult Height100Pct()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }

        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult ThemeWhite()
        {
            return View();
        }

        public ActionResult ThemeTraditional()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }

        public ActionResult TimeHeaderCellDuration()
        {
            return View();
        }

        public ActionResult ActiveAreas()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult HeaderAutoFit()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult Open()
        {
            return View();
        }

        public ActionResult EventArrangement()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult Today()
        {
            return View();
        }

        public ActionResult DaysResources()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult DayRange()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult AutoHide()
        {
            return View();
        }

        public ActionResult GoogleLike()
        {
            return View();
        }

        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult ThemeGreenWithBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult Outlook2000()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult Backend()
        {
            return new DayPilotCalendarControl(EventManager).CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new DayPilotNavigatorControl().CallBack(this);
        }

        public ActionResult New(int emailID, string partStart, string partEnd)
        {
            return View(
                new CreateInvitationInfo
                {
                    EmailID = emailID,
                    DateStart = partStart,
                    DateEnd = partEnd
                }
            );
        }

        private static T GetValueFromForm<T>(FormCollection form, String name, T defaultValue = default(T))
        {
            var obj = form[name];
            if (obj == null)
                return defaultValue;

            var returnType = typeof (T);
            if (returnType == typeof (String))
                return (T)(object)obj;
            if (returnType == typeof(DateTime))
            {
                return (T)(object)Convert.ToDateTime(obj);
            }
            if (returnType == typeof (int))
                return (T)(object)Convert.ToInt32(obj);
            throw new Exception("Invalid type provided");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {
            var dtFromDate = GetValueFromForm(form, "DateStart", DateTime.MinValue);
            var dtToDate = GetValueFromForm(form, "DateEnd", DateTime.MinValue);
            var strLocation = GetValueFromForm(form, "Location", String.Empty);
            var strStatus = GetValueFromForm(form, "Status", String.Empty);
            var strTransparency = GetValueFromForm(form, "TransparencyType", String.Empty);
            var strClass = GetValueFromForm(form, "Class", String.Empty);
            var strDescription = GetValueFromForm(form, "Description", String.Empty);
            var intPriority = GetValueFromForm(form, "Priority", 0);
            var intSequence = GetValueFromForm(form, "Sequence", 0);
            var strSummary = GetValueFromForm(form, "Summary", String.Empty);
            var strCategory = GetValueFromForm(form, "Category", String.Empty);
            var emailID = GetValueFromForm(form, "EmailID", 0);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var invitationRepository = new InvitationRepository(uow);
            Invitation invitation = new Invitation();
            invitationRepository.Add(invitation);

            var emailRepository = new EmailRepository(uow);
            Email existingEmail = emailRepository.GetByKey(emailID);
            existingEmail.StatusID = EmailStatusConstants.PROCESSED;
            existingEmail.Invitation = invitation;

            invitation.StartDate = dtFromDate;
            invitation.EndDate = dtToDate;
            invitation.Location = strLocation;
            invitation.Status = strStatus;
            invitation.Transparency = strTransparency;
            invitation.Class = strClass;
            invitation.Description = strDescription;
            invitation.Priority = intPriority;
            invitation.Sequence = intSequence;
            invitation.Summary = strSummary;
            invitation.Category = strCategory;
            uow.SaveChanges();

            EventManager.EventAdd(invitation);
            EventManager.Reload();

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        #endregion "Action"

        #region "Method"

        private void PopulateCalender()
        {
            
        }

        #endregion "Method"

        public class CreateInvitationInfo
        {
            public int EmailID;
            public string DateStart;
            public string DateEnd;
        }

    }
}
