using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Daemons;
using Data.Infrastructure;
using KwasantCore.StructureMap;
using KwasantWeb.App_Start;
using KwasantWeb.Controllers;

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
<<<<<<< HEAD
            ShnexyDbContext db = new ShnexyDbContext();
=======

            Database.SetInitializer(new ShnexyInitializer());
            KwasantDbContext db = new KwasantDbContext();
>>>>>>> dev
            db.Database.Initialize(true);


            var emailDaemon = new InboundEmail();

            //issues: doing it this way, you have to derive a class to create a seed file. seems like the EF6 seed file approach is best, but it's not getting called. wrong assembly name?
        }
    }
}
