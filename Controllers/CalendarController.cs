using System;
using System.Data;
using System.Linq;
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

        public ActionResult Index(int id)
        {
            Email email = db.Emails.Find(id);            

            //EmailAddress emailAddress = db.EmailAddresses.Find(email.Sender);
            //EmailAddress emailAddress = db.EmailAddresses.Where();

            //_emailAddRepo = new EmailAddressRepository(new UnitOfWork(new ShnexyDbContext()));
            //IEnumerable<EmailAddress> emailAddress  =  _emailAddRepo.GetAll();

            //ICollection<EmailAddress> ed= email.Bcc_Addresses.ToList();
             
            if (email == null)
            {
                return HttpNotFound();
            }
            else
            {
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
            DateTime start = Convert.ToDateTime(form["Start"]);
            DateTime end = Convert.ToDateTime(form["End"]);
            new EventManager(this).EventCreate(start, end, form["Text"], null);
            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
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
                        StartDate = (DateTime) e.Data["start"];
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
