using Data.Interfaces;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Navigator;
using StructureMap;
using System.Linq;
using KwasantCore.Services;

namespace KwasantWeb.Controllers.DayPilot
{ 
    public class DayPilotNavigatorControl : DayPilotNavigator
    {
        private readonly int _bookingRequestID;
        public DayPilotNavigatorControl(int bookingRequestID)
        {
            _bookingRequestID = bookingRequestID;
        }

        protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs a)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string userId = new BookingRequest().GetUserId(uow.BookingRequestRepository, _bookingRequestID);
                var events = uow.EventRepository.GetAll().Where(s => s.BookingRequest.User.Id == userId).ToList();
                Events = events.Select(e =>
                new
                {
                    start = e.StartDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffff"),
                    end = e.EndDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffff"),
                    text = e.Summary,
                    id = e.Id,
                    allday = e.IsAllDay
                });

                DataStartField = "start";
                DataEndField = "end";
                DataIdField = "id";
            }
        }
    }

}