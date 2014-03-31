using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;

using Shnexy.Models;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;

using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using DayPilot.Web.Mvc.Events.Common;
using DayPilot.Web.Mvc.Events.Navigator;
using DayPilot.Web.Mvc.Json;
using BeforeCellRenderArgs = DayPilot.Web.Mvc.Events.Calendar.BeforeCellRenderArgs;
using TimeRangeSelectedArgs = DayPilot.Web.Mvc.Events.Calendar.TimeRangeSelectedArgs;


namespace Shnexy.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {
        private ShnexyDbContext db = new ShnexyDbContext();

        IEmailAddressRepository _emailAddRepo;

        public ActionResult Index(int id = 0)
        {

            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Email email = db.Emails.Find(id);

            List<EmailAddress> emailSenderList = new List<EmailAddress>();
            StringBuilder sb = new StringBuilder("");

            //Get Sender
            sb.Append("SELECT dbo.EmailAddresses.Id, DisplayName, Address FROM dbo.EmailAddresses");
            sb.AppendFormat(" INNER JOIN dbo.Emails AS E ON E.Sender_Id= dbo.EmailAddresses.Id WHERE E.Id={0}", email.Id);

            emailSenderList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

            if (emailSenderList != null && emailSenderList.Count > 0)
            {
                email.Sender = emailSenderList[0];
            }

            //Get To Addresses
            sb = new StringBuilder("");
            sb.AppendFormat(" SELECT Id, DisplayName, Address FROM dbo.EmailAddresses WHERE Email_Id2={0}", email.Id);

            List<EmailAddress> emailToEmailAddressList = new List<EmailAddress>();

            emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

            StringBuilder sbToEmailAddressList = new StringBuilder("");

            if (emailToEmailAddressList != null && emailToEmailAddressList.Count > 0)
            {
                email.To_Addresses = emailToEmailAddressList;

                foreach (EmailAddress emailAddress in emailToEmailAddressList)
                {
                    if (sbToEmailAddressList.Length == 0)
                    {
                        sbToEmailAddressList.Append(emailAddress.Address);
                    }
                    else
                    {
                        sbToEmailAddressList.Append(",");
                        sbToEmailAddressList.Append(emailAddress.Address);
                    }
                }
            }


            //Get CC Addresses
            sb = new StringBuilder("");
            sb.AppendFormat(" SELECT Id, DisplayName, Address FROM dbo.EmailAddresses WHERE Email_Id1={0}", email.Id);

            List<EmailAddress> emailCCEmailAddressList = new List<EmailAddress>();

            emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

            StringBuilder sbCCEmailAddressList = new StringBuilder("");

            if (emailCCEmailAddressList != null && emailCCEmailAddressList.Count > 0)
            {
                email.CC_Addresses = emailCCEmailAddressList;

                foreach (EmailAddress emailAddress in emailCCEmailAddressList)
                {
                    if (sbCCEmailAddressList.Length == 0)
                    {
                        sbCCEmailAddressList.Append(emailAddress.Address);
                    }
                    else
                    {
                        sbCCEmailAddressList.Append(",");
                        sbCCEmailAddressList.Append(emailAddress.Address);
                    }
                }
            }

            //Get BCC Addresses
            sb = new StringBuilder("");
            sb.AppendFormat(" SELECT Id, DisplayName, Address FROM dbo.EmailAddresses WHERE Email_Id={0}", email.Id);

            List<EmailAddress> emailBCCEmailAddressList = new List<EmailAddress>();

            emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

            StringBuilder sbBCCEmailAddressList = new StringBuilder("");

            if (emailBCCEmailAddressList != null && emailBCCEmailAddressList.Count > 0)
            {
                email.Bcc_Addresses = emailBCCEmailAddressList;

                foreach (EmailAddress emailAddress in emailBCCEmailAddressList)
                {
                    if (sbBCCEmailAddressList.Length == 0)
                    {
                        sbBCCEmailAddressList.Append(emailAddress.Address);
                    }
                    else
                    {
                        sbBCCEmailAddressList.Append(",");
                        sbBCCEmailAddressList.Append(emailAddress.Address);
                    }
                }
            }



            if (email == null)
            {
                return HttpNotFound();
            }
            else
            {
                ViewData["ToEmailAddresses"] = sbToEmailAddressList.ToString();
                ViewData["CCEmailAddresses"] = sbCCEmailAddressList.ToString();
                ViewData["BCCEmailAddresses"] = sbBCCEmailAddressList.ToString();
                return View(email);
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
            return new Dpc().CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new Dpn().CallBack(this);
        }

        public ActionResult New(string id)
        {
            return View(new EventManager.Event
            {
                Start = Convert.ToDateTime(Request.QueryString["start"]),
                End = Convert.ToDateTime(Request.QueryString["end"])
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {

            String strEventName = String.Empty;
            TimeSpan tsDuration = new TimeSpan();
            String strLocation = String.Empty;
            Boolean bIsAllDay = false;
            int intStatus = 0;
            int intTransparency = 0;
            String strClass = String.Empty;
            String strDescription = String.Empty;
            int intPriority = 0;
            int intSequence = 0;
            int intLine = 0;
            int intColumn = 0;

            strEventName = !String.IsNullOrEmpty(form["Name"]) ? form["Name"].ToString() : String.Empty;
            strLocation = !String.IsNullOrEmpty(form["Location"]) ? form["Location"].ToString() : String.Empty;
            intStatus = !String.IsNullOrEmpty(form["Status"]) ? Convert.ToInt32(form["Status"]) : 0;
            intTransparency = !String.IsNullOrEmpty(form["TransparencyType"]) ? Convert.ToInt32(form["TransparencyType"]) : 0;
            intPriority = !String.IsNullOrEmpty(form["Priority"]) ? Convert.ToInt32(form["Priority"]) : 0 ;
            intSequence = !String.IsNullOrEmpty(form["Sequence"]) ? Convert.ToInt32(form["Sequence"]) : 0 ;
            intLine = !String.IsNullOrEmpty(form["Line"]) ? Convert.ToInt32(form["Line"]) : 0 ;
            intColumn = !String.IsNullOrEmpty(form["Column"]) ? Convert.ToInt32(form["Column"]) : 0;

            if (String.IsNullOrEmpty(form["Duration"]))
            {
            }
            else
            {
                long lngDuration = Convert.ToInt64(form["Duration"]);
                tsDuration = TimeSpan.FromTicks(lngDuration);
            }            

            if (String.IsNullOrEmpty(form["chkIsAllDay"]))
            {

            }
            else
            {
                String strCheckAllDay = form["chkIsAllDay"].ToString();
                String strTemp = String.Empty;

                if (strCheckAllDay.IndexOf(',') > -1)
                {                    
                  String [] arrCheckAllDay = strCheckAllDay.Split(',');
                  strTemp = arrCheckAllDay[0];
                }
                else
                {
                    strTemp = strCheckAllDay;
                }

                bIsAllDay = Convert.ToBoolean(strTemp);
            }


            if (intStatus == 0 || intTransparency == 0 || intPriority == 0 || intSequence == 0 || intLine == 0 || intColumn == 0 || !bIsAllDay)
                return View();            

            Event evt = new Event();

            evt.Name = strEventName;
            evt.Duration = tsDuration;
            evt.Location = strLocation;
            evt.IsAllDay = bIsAllDay;

            switch (intStatus)
            {
                case 1:
                    evt.Status = DDay.iCal.EventStatus.Tentative;
                    break;
                case 2:
                    evt.Status = DDay.iCal.EventStatus.Confirmed;
                    break;
                case 3:
                    evt.Status = DDay.iCal.EventStatus.Cancelled;
                    break;
            }

            switch (intTransparency)
            {
                case 1:
                    evt.Transparency = DDay.iCal.TransparencyType.Opaque;
                    break;
                case 2:
                    evt.Transparency = DDay.iCal.TransparencyType.Transparent;
                    break;                
            }

            evt.Class = strClass;
            evt.Description = strDescription;
            evt.Priority = intPriority;
            evt.Sequence=intSequence;
            evt.Line = intLine;
            evt.Column = intColumn;

            //DateTime end = Convert.ToDateTime(form["End"]);
            //new EventManager(this).EventCreate(start, end, form["Text"], null);
            //return JavaScript(SimpleJsonSerializer.Serialize("OK"));

            IEventRepository eventRepo = new EventRepository(new UnitOfWork(new ShnexyDbContext()));
            eventRepo.Add(evt);

            //db.Events.Add(evt);
            db.SaveChanges();

            return View();
        }

        public class Dpn : DayPilotNavigator
        {
            protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs visibleRangeChangedArgs)
            {
                // this select is a really bad example, no where clause
                if (Id == "dpn_recurring")
                {
                    Events = new EventManager(Controller, "recurring").Data.AsEnumerable();
                    DataRecurrenceField = "recurrence";
                }
                else
                {
                    Events = new EventManager(Controller).Data.AsEnumerable();
                }

                DataStartField = "start";
                DataEndField = "end";
                DataIdField = "id";

            }
        }

        public class Dpc : DayPilotCalendar
        {
            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {
                new EventManager(Controller).EventCreate(e.Start, e.End, "Default name", e.Resource);
                Update();
            }

            protected override void OnEventMove(DayPilot.Web.Mvc.Events.Calendar.EventMoveArgs e)
            {
                if (new EventManager(Controller).Get(e.Id) != null)
                {
                    new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                }
                else // external drag&drop
                {
                    new EventManager(Controller).EventCreate(e.NewStart, e.NewEnd, e.Text, e.NewResource, e.Id);
                }

                Update();
            }

            protected override void OnEventClick(EventClickArgs e)
            {
                UpdateWithMessage("Event clicked: " + e.Text);
                //Redirect("http://www.daypilot.org/");
            }

            protected override void OnEventDelete(EventDeleteArgs e)
            {
                new EventManager(Controller).EventDelete(e.Id);
                Update();
            }

            protected override void OnEventResize(DayPilot.Web.Mvc.Events.Calendar.EventResizeArgs e)
            {
                new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                Update();
            }

            protected override void OnEventBubble(EventBubbleArgs e)
            {
                e.BubbleHtml = "This is an event bubble for id: " + e.Id;
            }

            protected override void OnEventMenuClick(EventMenuClickArgs e)
            {
                switch (e.Command)
                {
                    case "Delete":
                        new EventManager(Controller).EventDelete(e.Id);
                        Update();
                        break;

                }
            }

            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "navigate":
                        StartDate = (DateTime)e.Data["start"];
                        Update(CallBackUpdateType.Full);
                        break;

                    case "refresh":
                        UpdateWithMessage("Refreshed");
                        break;

                    case "selected":
                        if (SelectedEvents.Count > 0)
                        {
                            EventInfo ei = SelectedEvents[0];
                            SelectedEvents.RemoveAt(0);
                            UpdateWithMessage("Event removed from selection: " + ei.Text);
                        }

                        break;

                    case "delete":
                        string id = (string)e.Data["id"];
                        new EventManager(Controller).EventDelete(id);
                        Update(CallBackUpdateType.EventsOnly);
                        break;

                }
            }

            protected override void OnBeforeCellRender(BeforeCellRenderArgs e)
            {
                if (Id == "dpc_today")
                {
                    if (e.Start.Date == DateTime.Today)
                    {
                        if (e.IsBusiness)
                        {
                            e.BackgroundColor = "#ffaaaa";
                        }
                        else
                        {
                            e.BackgroundColor = "#ff6666";
                        }
                    }
                }

            }

            protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
            {

                if (Id == "dpcg")  // Calendar/GoogleLike
                {
                    if (e.Id == "6")
                    {
                        e.BorderColor = "#1AAFE0";
                        e.BackgroundColor = "#90D8F2";
                    }
                    if (e.Id == "8")
                    {
                        e.BorderColor = "#068c14";
                        e.BackgroundColor = "#08b81b";
                    }
                    if (e.Id == "2")
                    {
                        e.BorderColor = "#990607";
                        e.BackgroundColor = "#f60e13";
                    }
                }
                else if (Id == "dpc_menu")  // Calendar/ContextMenu
                {
                    if (e.Id == "7")
                    {
                        e.ContextMenuClientName = "menu2";
                    }
                }
                else if (Id == "dpc_areas")  // Calendar/ActiveAreas
                {
                    e.CssClass = "calendar_white_event_withheader";

                    e.Areas.Add(new Area().Right(3).Top(3).Width(15).Height(15).CssClass("event_action_delete").JavaScript("dpc_areas.eventDeleteCallBack(e);"));
                    e.Areas.Add(new Area().Right(20).Top(3).Width(15).Height(15).CssClass("event_action_menu").JavaScript("dpc_areas.bubble.showEvent(e, true);"));
                    e.Areas.Add(new Area().Left(0).Bottom(5).Right(0).Height(5).CssClass("event_action_bottomdrag").ResizeEnd());
                    e.Areas.Add(new Area().Left(15).Top(1).Right(46).Height(11).CssClass("event_action_move").Move());
                }

                if (e.Id == "7")
                {
                    e.DurationBarColor = "red";
                }

                if (e.Recurrent)
                {
                    e.InnerHtml += " (R)";
                }
            }

            protected override void OnInit(InitArgs initArgs)
            {

                //Thread.Sleep(5000);

                UpdateWithMessage("Welcome!", CallBackUpdateType.Full);

                if (Id == "days_resources")
                {
                    Columns.Clear();
                    Column today = new Column(DateTime.Today.ToShortDateString(), DateTime.Today.ToString("s"));
                    today.Children.Add("A", "a", DateTime.Today);
                    today.Children.Add("B", "b", DateTime.Today);
                    Columns.Add(today);

                    Column tomorrow = new Column(DateTime.Today.AddDays(1).ToShortDateString(), DateTime.Today.AddDays(1).ToString("s"));
                    tomorrow.Children.Add("A", "a", DateTime.Today.AddDays(1));
                    tomorrow.Children.Add("B", "b", DateTime.Today.AddDays(1));
                    Columns.Add(tomorrow);

                }
                else if (Id == "resources")
                {
                    Columns.Clear();
                    Columns.Add("A", "A");
                    Columns.Add("B", "B");
                    Columns.Add("C", "C");
                }
            }

            protected override void OnBeforeHeaderRender(BeforeHeaderRenderArgs e)
            {
                if (Id == "dpc_areas")
                {
                    e.Areas.Add(new Area().Right(1).Top(0).Width(17).Bottom(1).CssClass("resource_action_menu").Html("<div><div></div></div>").JavaScript("alert(e.date);"));
                }
                if (Id == "dpc_autofit")
                {
                    e.InnerHtml += " adding some longer text so the autofit can be tested";
                }

            }
            protected override void OnBeforeTimeHeaderRender(BeforeTimeHeaderRenderArgs e)
            {
            }

            protected override void OnFinish()
            {
                // only load the data if an update was requested by an Update() call
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                // this select is a really bad example, no where clause
                if (Id == "dpc_recurring")
                {
                    Events = new EventManager(Controller, "recurring").Data.AsEnumerable();
                    DataRecurrenceField = "recurrence";
                }
                else
                {
                    Events = new EventManager(Controller).Data.AsEnumerable();
                }

                DataStartField = "start";
                DataEndField = "end";
                DataTextField = "text";
                DataIdField = "id";
                DataResourceField = "resource";

                DataAllDayField = "allday";
                //DataTagFields = "id, name";

            }

        }

    }
}
