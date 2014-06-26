﻿using System.Net;
using System.Web.Mvc;
using Data.Interfaces;
using Data.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Managers.IdentityManager;
using KwasantCore.Services;
using KwasantWeb.Controllers.DayPilot;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    [KwasantAuthorize(Roles = "Admin")]
    public class CalendarController : Controller
    {

        #region "Action"

        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IBookingRequestRepository bookingRequestRepository = uow.BookingRequestRepository;
                var bookingRequestDO = bookingRequestRepository.GetByKey(id);
                if (bookingRequestDO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                return View(bookingRequestDO);
            }
        }





        #endregion "Action"

        #region "DayPilot-Related Methods"
        public ActionResult Day(int bookingRequestID)
        {
            return new DayPilotCalendarControl(bookingRequestID).CallBack(this);
        }

        public ActionResult Month(int bookingRequestID)
        {
            return new DayPilotMonthControl(bookingRequestID).CallBack(this);
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

        public ActionResult Backend(int bookingRequestID)
        {
            return new DayPilotCalendarControl(bookingRequestID).CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new DayPilotNavigatorControl().CallBack(this);
        }
        #endregion "DayPilot-Related Methods"

        #region "Quick Copy Methods"
        [HttpGet]
        public ActionResult ProcessQuickCopy(string copyType,string selectedText)
        {
            string value = (new Calendar()).ProcessQuickCopy(copyType, selectedText);
            string status = "valid";
            if (value == "Invalid Selection") { status = "invalid"; }
            var jsonResult = Json(new { status = status, value = value, copytype = copyType }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        #endregion "Quick Copy Methods"
    }
}
