using System;
using System.Drawing;
using Data.Constants;
using Data.Models;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using DayPilot.Web.Mvc.Events.Common;
using Shnexy.Controllers.Models;
using Calendar = Data.Models.Calendar;

namespace Shnexy.Controllers.DayPilot
{
    public class DayPilotCalendarControl : DayPilotCalendar
    {
        private readonly Calendar _calendar;
        public DayPilotCalendarControl(Calendar calendar)
        {
            _calendar = calendar;
        }

        protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
        {
            _calendar.AddEvent(new EventDO
            {
                StartDate = e.Start,
                EndDate = e.End,
                Summary = "Click to Open Form"
            });
            
            Update();
        }

        protected override void OnEventMove(EventMoveArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
            //if (new EventManager(Controller).Get(e.Id) != null)
            //{
            //    new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
            //    MoveUpdateEventFile(e.Id, e.NewStart, e.NewEnd);
            //}
            //else // external drag&drop
            //{
            //    new EventManager(Controller).EventCreate(e.NewStart, e.NewEnd, e.Text, e.NewResource, e.Id);
            //    MoveUpdateEventFile(e.Id, e.NewStart, e.NewEnd);
            //}

            Update();
        }
        
        protected override void OnEventDelete(EventDeleteArgs e)
        {
            _calendar.DeleteEvent(e.Id);
            Update();
        }

        protected override void OnEventResize(EventResizeArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
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

                    int intResultId;
                    Boolean blnResult;

                    blnResult = Int32.TryParse(e.Id, out intResultId);

                    _calendar.DeleteEvent(e.Id);
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
                    Update();
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
                    _calendar.DeleteEvent(id);
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
            var eventDO = (e.DataItem.Source as EventDO);
            if (eventDO == null)
                return;
            if (eventDO.StatusID == EmailStatusConstants.EVENT_UNSET)
            {
                e.BackgroundColor = "#B33A3A";
            }
        }

        protected override void OnInit(InitArgs initArgs)
        {
            //Thread.Sleep(5000);
            Update(CallBackUpdateType.Full);

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

            //CalendarController.EventData._dtCalenderEndDate = e.Date;
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
            
            DataStartField = "StartDate";
            DataEndField = "EndDate";
            DataTextField = "Summary";
            DataIdField = "EventID";
            DataAllDayField = "IsAllDay";

            Events = _calendar.EventsList;
        }
    }
}