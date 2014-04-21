using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Models;
using Data;
using DayPilot.Web;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using Shnexy.Controllers.DayPilot;
using DayPilot.Web.Mvc.Events.Calendar;
using MonthNamespace = DayPilot.Web.Mvc.Events.Month;

namespace Shnexy.Controllers.DayPilot
{ 
    public class DayPilotMonthControl : DayPilotMonth
    {
        private readonly Calendar _calendar;
        public DayPilotMonthControl(Calendar calendar)
        {
            _calendar = calendar;
        }

        protected override void OnInit(MonthNamespace.InitArgs e)
        {
            Update();
        }

        protected override void OnEventResize(MonthNamespace.EventResizeArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }        

        protected override void OnEventMove(MonthNamespace.EventMoveArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }

        protected override void OnEventMenuClick(MonthNamespace.EventMenuClickArgs e)
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

        protected override void OnTimeRangeSelected(MonthNamespace.TimeRangeSelectedArgs e)
        {
            _calendar.AddEvent(new EventDO
            {
                StartDate = e.Start,
                EndDate = e.End,
                Summary = "Click to Open Form"
            });

            Update();
        }

        protected override void OnBeforeEventRender(MonthNamespace.BeforeEventRenderArgs e)
        {
            e.Areas.Add(new Area().Right(3).Top(3).Width(15).Height(15).CssClass("event_action_delete").JavaScript("switcher.active.control.commandCallBack('delete', {'e': e});"));
        }

        protected override void OnCommand(MonthNamespace.CommandArgs e)
        {
            switch (e.Command)
            {
                case "navigate":
                    StartDate = (DateTime)e.Data["day"];
                    Update(CallBackUpdateType.Full);
                    break;
                case "refresh":
                    Update(CallBackUpdateType.EventsOnly);
                    break;
                case "delete":
                   string id = (string)e.Data["id"];
                   _calendar.DeleteEvent(id);
                    Update(CallBackUpdateType.EventsOnly);
                    break;
            }
        }

        protected override void OnFinish()
        {
            if (UpdateType == CallBackUpdateType.None)
            {
                return;
            }

            DataStartField = "StartDate";
            DataEndField = "EndDate";
            DataTextField = "Summary";
            DataIdField = "InvitationID";

            Events = _calendar.EventsList;
            

        }
    }
}