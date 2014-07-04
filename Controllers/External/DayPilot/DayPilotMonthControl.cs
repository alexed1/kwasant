using System;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using StructureMap;

namespace KwasantWeb.Controllers.DayPilot
{ 
    public class DayPilotMonthControl : DayPilotMonth
    {
        private readonly int _bookingRequestID;
        public DayPilotMonthControl(int bookingRequestID)
        {
            _bookingRequestID = bookingRequestID;
        }

        protected override void OnInit(global::DayPilot.Web.Mvc.Events.Month.InitArgs e)
        {
            Update();
        }

        protected override void OnEventResize(global::DayPilot.Web.Mvc.Events.Month.EventResizeArgs e)
        {
            MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
        }        

        protected override void OnEventMove(global::DayPilot.Web.Mvc.Events.Month.EventMoveArgs e)
        {
            MoveEvent(e.Id, e.NewStart, e.NewEnd);
            Update();
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

        protected override void OnEventMenuClick(global::DayPilot.Web.Mvc.Events.Month.EventMenuClickArgs e)
        {
            switch (e.Command)
            {
                case "Delete":
                    DeleteEvent(e.Id);
                    Update();
                    break;
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

        protected override void OnTimeRangeSelected(global::DayPilot.Web.Mvc.Events.Month.TimeRangeSelectedArgs e)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EventRepository.Add(new EventDO
                {
                    StartDate = e.Start,
                    EndDate = e.End,
                    Summary = "Click to Open Form"
                });
                uow.SaveChanges();
            }
            
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
                    DeleteEvent(id);
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