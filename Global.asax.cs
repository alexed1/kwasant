﻿using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Data.Infrastructure;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.ModelBinders;
using KwasantCore.Services;
using KwasantCore.Managers;
using KwasantCore.StructureMap;
using KwasantWeb.App_Start;
using KwasantWeb.ViewModels;
using Utilities.Logging;

namespace KwasantWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static bool _IsInitialised;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "live"

            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            Utilities.Server.ServerPhysicalPath = Server.MapPath("~");

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

            
            AnalyticsManager curAnalyticsManager = new AnalyticsManager();
            curAnalyticsManager.SubscribeToAlerts();

            AlertReporter curReporter = new AlertReporter();
            curReporter.SubscribeToAlerts();

//            ModelBinders.Binders.Add(typeof(EventViewModel), new KwasantDateBinder());
            ModelBinders.Binders.Add(typeof(DateTimeOffset), new KwasantDateBinder());
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            Logger.GetLogger().Error("Critical internal error occured.", exception);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!_IsInitialised)
            {
                SetServerUrl(HttpContext.Current);
                Utilities.Server.IsDevMode = Utilities.Server.ServerHostName.Contains("localhost");
                _IsInitialised = true;
            }
        }

        private static void SetServerUrl(HttpContext context)
        {
            string port = context.Request.ServerVariables["SERVER_PORT"];
            if (port == null || port == "80" || port == "443")
                port = "";
            else
                port = ":" + port;

            string protocol = context.Request.ServerVariables["SERVER_PORT_SECURE"];
            if (protocol == null || protocol == "0")
                protocol = "http://";
            else
                protocol = "https://";

            // *** Figure out the base Url which points at the application's root
            Utilities.Server.ServerHostName = context.Request.ServerVariables["SERVER_NAME"];
            string url = protocol + context.Request.ServerVariables["SERVER_NAME"] + port + context.Request.ApplicationPath;
            Utilities.Server.ServerUrl = url;
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
