using System;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using DayPilot.Web.Mvc.Events.Common;
using StructureMap;

namespace KwasantWeb.Controllers.DayPilot
{
    public class DayPilotCalendarControl : DayPilotCalendar
    {
        private readonly int _bookingRequestID;
        public DayPilotCalendarControl(int bookingRequestID)
        {
            _bookingRequestID = bookingRequestID;
        }

        private static void MoveEvent(string id, DateTime newStart, DateTime newEnd)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetByKey(id);
                eventDO.StartDate = newStart;
                eventDO.EndDate = newEnd;
                uow.SaveChanges();
            }
        }

        private static void DeleteEvent(string id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetByKey(id);
                uow.EventRepository.Remove(eventDO);
                uow.SaveChanges();
            }
        }

        protected override void OnEventMove(EventMoveArgs e)
        {
            MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }
        
        protected override void OnEventDelete(EventDeleteArgs e)
        {
            DeleteEvent(e.Id);
            Update();
        }

        protected override void OnEventResize(EventResizeArgs e)
        {
            MoveEvent(e.Id, e.NewStart, e.NewEnd);
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
                    DeleteEvent(e.Id);
                    Update();
                    break;
            }
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
                    UpdateWithMessage("Changes Saved.");
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
                    DeleteEvent(id);
                    Update(CallBackUpdateType.EventsOnly);
                    break;

            }
        }

        protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
        {
            e.Areas.Add(new Area().Right(3).Top(3).Width(15).Height(15).CssClass("event_action_delete").JavaScript("eventDelete(e);"));
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
            
            DataStartField = GetPropertyName(ev => ev.StartDate);
            DataEndField = GetPropertyName(ev => ev.EndDate);
            DataTextField = GetPropertyName(ev => ev.Summary);
            DataIdField = GetPropertyName(ev => ev.Id);
            DataAllDayField = GetPropertyName(ev => ev.IsAllDay);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Events = uow.EventRepository.GetQuery().Where(e => e.BookingRequest.Id == _bookingRequestID).ToList();
            }
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