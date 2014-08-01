using System;
using System.Linq.Expressions;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using DayPilot.Web.Mvc.Events.Common;
using KwasantWeb.Controllers.External.DayPilot.Providers;

namespace KwasantWeb.Controllers.External.DayPilot
{
    public class KwasantCalendarController : DayPilotCalendar
    {
        private readonly IEventDataProvider _provider;
        public KwasantCalendarController(IEventDataProvider provider)
        {
            _provider = provider;
        }

        protected override void OnEventBubble(EventBubbleArgs e)
        {
            e.BubbleHtml = _provider.GetTimeslotBubbleText(int.Parse(e.Id));
        }

        protected override void OnCommand(CommandArgs e)
        {
            switch (e.Command)
            {
                case "navigate":
                    if (e.Data["day"] != null)
                    {
                        StartDate = (DateTime)e.Data["day"];
                    }
                    Update(CallBackUpdateType.Full);
                    break;

                case "refresh":
                    Update(CallBackUpdateType.Full);
                    break;

                case "selected":
                    if (SelectedEvents.Count > 0)
                    {
                        EventInfo ei = SelectedEvents[0];
                        SelectedEvents.RemoveAt(0);
                        UpdateWithMessage("Event removed from selection: " + ei.Text);
                    }

                    break;
            }
        }


        protected override void OnFinish()
        {
            // only load the data if an update was requested by an Update() call
            if (UpdateType == CallBackUpdateType.None)
            {
                return;
            }

            DataStartField = GetPropertyName(ev => ev.StartDate);
            DataEndField = GetPropertyName(ev => ev.EndDate);
            DataTextField = GetPropertyName(ev => ev.Text);
            DataIdField = GetPropertyName(ev => ev.Id);
            DataAllDayField = GetPropertyName(ev => ev.IsAllDay);
            DataTagFields = GetPropertyName(ev => ev.Tag);

            Events = _provider.LoadData();
        }

        protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
        {
            _provider.BeforeCellRender(e);
            base.OnBeforeEventRender(e);
        }

        //This creates a statically typed reference to our supplied property. If we change it in the future, it won't compile (so it won't break at runtime).
        //Changing the property with tools like resharper will automatically update here.
        private string GetPropertyName<T>(Expression<Func<DayPilotTimeslotInfo, T>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
                return (expression.Body as dynamic).Member.Name;

            throw new Exception("Cannot contain complex expressions. An example of a supported expression is 'ev => ev.Id'");
        }

    }
}