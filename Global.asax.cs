using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.StructureMap;

namespace Shnexy
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
            StructureMapBootStrapper.ConfigureDependencies("test"); //set to either "test" or "dev"
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

            Database.SetInitializer(new ShnexyInitializer());
            ShnexyDbContext db = new ShnexyDbContext();
            db.Database.Initialize(true);

           


            //issues: doing it this way, you have to derive a class to create a seed file. seems like the EF6 seed file approach is best, but it's not getting called. wrong assembly name?
        }
    }
}
