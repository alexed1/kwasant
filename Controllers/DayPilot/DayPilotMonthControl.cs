using System;
using System.Linq.Expressions;
using Data.Entities;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using KwasantCore.Services;

namespace KwasantWeb.Controllers.DayPilot
{ 
    public class DayPilotMonthControl : DayPilotMonth
    {
        private readonly Calendar _calendar;
        public DayPilotMonthControl(Calendar calendar)
        {
            _calendar = calendar;
        }

        protected override void OnInit(global::DayPilot.Web.Mvc.Events.Month.InitArgs e)
        {
            Update();
        }

        protected override void OnEventResize(global::DayPilot.Web.Mvc.Events.Month.EventResizeArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }        

        protected override void OnEventMove(global::DayPilot.Web.Mvc.Events.Month.EventMoveArgs e)
        {
            _calendar.MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }

        protected override void OnEventMenuClick(global::DayPilot.Web.Mvc.Events.Month.EventMenuClickArgs e)
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

        protected override void OnTimeRangeSelected(global::DayPilot.Web.Mvc.Events.Month.TimeRangeSelectedArgs e)
        {
            _calendar.AddEvent(new EventDO
            {
                StartDate = e.Start,
                EndDate = e.End,
                Summary = "Click to Open Form"
            });

            Update();
        }

        protected override void OnBeforeEventRender(global::DayPilot.Web.Mvc.Events.Month.BeforeEventRenderArgs e)
        {
            e.Areas.Add(new Area().Right(3).Top(3).Width(15).Height(15).CssClass("event_action_delete").JavaScript("eventDelete(e);"));
        }

        protected override void OnCommand(global::DayPilot.Web.Mvc.Events.Month.CommandArgs e)
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

            DataStartField = GetPropertyName(ev => ev.StartDate);
            DataEndField = GetPropertyName(ev => ev.EndDate);
            DataTextField = GetPropertyName(ev => ev.Summary);
            DataIdField = GetPropertyName(ev => ev.Id);
            Events = _calendar.EventsList;
        }

        //This creates a statically typed reference to our supplied property. If we change it in the future, it won't compile (so it won't break at runtime).
        //Changing the property with tools like resharper will automatically update here.
        private string GetPropertyName<T>(Expression<Func<EventDO, T>> expression)
        {
            if(expression.Body.NodeType == ExpressionType.MemberAccess)
                return (expression.Body as dynamic).Member.Name;

            throw new Exception("Cannot contain complex expressions. An example of a supported expression is 'ev => ev.Id'");
        }
    }
}