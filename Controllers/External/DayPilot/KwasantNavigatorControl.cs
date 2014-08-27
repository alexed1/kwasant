using System;
using System.Linq.Expressions;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Navigator;
using KwasantWeb.Controllers.External.DayPilot.Providers;

namespace KwasantWeb.Controllers.External.DayPilot
{
    public class KwasantNavigatorControl : DayPilotNavigator
    {        
        private readonly IEventDataProvider _provider;

        public KwasantNavigatorControl(IEventDataProvider provider)
        {
            _provider = provider;
        }

        protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs a)
        {            
            DataStartField = GetPropertyName(ev => ev.StartDate);
            DataEndField = GetPropertyName(ev => ev.EndDate);
            DataIdField = GetPropertyName(ev => ev.Id);

            Events = _provider.LoadData();
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