using System.Data;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Navigator;
using Shnexy.Controllers.Data;

namespace Shnexy.Controllers.DayPilot
{
    public class DayPilotNavigatorControl : DayPilotNavigator
    {
        //protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs visibleRangeChangedArgs)
        //{
        //    // this select is a really bad example, no where clause
        //    if (Id == "dpn_recurring")
        //    {
        //        Events = new EventManager(Controller, "recurring").Data.AsEnumerable();
        //        DataRecurrenceField = "recurrence";
        //    }
        //    else
        //    {
        //        Events = new EventManager(Controller).Data.AsEnumerable();
        //    }

        //    DataStartField = "start";
        //    DataEndField = "end";
        //    DataIdField = "id";
        //}
    }

}