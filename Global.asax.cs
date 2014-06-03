using System;
using System.Configuration;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Daemons;
using Data.Infrastructure;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantWeb.App_Start;
using KwasantWeb.Controllers;
using FluentValidation;
using Utilities.Logging;

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
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "live"
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

            //Database.SetInitializer(new ShnexyInitializer());
            KwasanttDbContext db = new KwasanttDbContext();
            db.Database.Initialize(true);

            //AutoMapper create map configuration
            AutoMapperBootStrapper.ConfigureAutoMapper();

            Logger.GetLogger().Info("Kwasant web starting...");

            var baseURL = ConfigurationManager.AppSettings["BasePageURL"];
            if (!String.IsNullOrEmpty(baseURL))
            {
                if (Uri.IsWellFormedUriString(baseURL, UriKind.Absolute))
                    Email.InitialiseWebhook(baseURL + "MandrillWebhook/");
                else
                    throw new Exception("Invalid BasePageURL (check web.config)");
            }


            CommunicationManager curCommManager = new CommunicationManager();
            curCommManager.SubscribeToAlerts();
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            Logger.GetLogger().Error("Critical internal error occured.", exception);
        }

        public void Application_End()
        {
            Logger.GetLogger().Info("Kwasant web shutting down...");

            // This will give LE background thread some time to finish sending messages to Logentries.
            var numWaits = 3;
            while (!LogentriesCore.Net.AsyncLogger.AreAllQueuesEmpty(TimeSpan.FromSeconds(5)) && numWaits > 0)
                numWaits--;
        }
    }
}
