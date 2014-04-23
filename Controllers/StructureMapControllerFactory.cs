using System;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace Shnexy.Controllers
{
    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        #region Method

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == typeof (CalendarController))
                return new CalendarController();

            return ObjectFactory.GetInstance(controllerType) as IController;
        }

        #endregion
    }
}