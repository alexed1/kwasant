using System.Web.Mvc;
using System.Web.Routing;

namespace KwasantWeb.App_Start
{
    public class RouteConfig
    {
        public const string ShowClarificationResponseUrl = "crr";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ShowClarificationResponse",
                url: ShowClarificationResponseUrl,
                defaults: new { controller = "ClarificationRequest", action = "ShowClarificationResponse", enc = UrlParameter.Optional });

            routes.MapRoute(
                name: "GoogleCalendar",
                url: "GoogleCalendar/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "KwasantWeb.Controllers.GoogleCalendar" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional});
        }
    }
}
