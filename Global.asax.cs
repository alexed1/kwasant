using System;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Daemons;
using Data.Infrastructure;
using KwasantCore.StructureMap;
using KwasantWeb.App_Start;
using KwasantWeb.Controllers;
using FluentValidation;
using UtilitiesLib.Logging;

namespace KwasantWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies("dev"); //set to either "test" or "dev"
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

            Database.SetInitializer(new ShnexyInitializer());
            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);            

            Logger.GetLogger().Info("Kwasant web starting...");

            //issues: doing it this way, you have to derive a class to create a seed file. seems like the EF6 seed file approach is best, but it's not getting called. wrong assembly name?
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            Logger.GetLogger().Error("Critical internal error occured.", exception);
        }
    }
}
