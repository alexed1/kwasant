using System.Web.Optimization;

namespace KwasantWeb.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Scripts
            bundles.Add(new ScriptBundle("~/bundles/js/modernizr").Include(
                "~/Content/js/modernizr.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                "~/Content/js/jquery.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
                "~/Content/js/bootstrap.js",
                "~/Content/js/bootstrap-responsive.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/colorbox").Include(
                "~/Content/js/jquery.colorbox.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/waypoints").Include(
                "~/Content/js/waypoints.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/placeholder").Include(
                "~/Content/js/placeholder.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/main").Include(
                "~/Content/js/main.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/common").Include(
                "~/Content/js/common.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/daypilot").Include(
                "~/Content/js/Daypilot/daypilot-common.src.js",
                "~/Content/js/Daypilot/daypilot-bubble.src.js",
                "~/Content/js/Daypilot/daypilot-calendar.src.js",
                "~/Content/js/Daypilot/daypilot-datepicker.src.js",
                "~/Content/js/Daypilot/daypilot-menu.src.js",
                "~/Content/js/Daypilot/daypilot-modal.src.js",
                "~/Content/js/Daypilot/daypilot-month.src.js",
                "~/Content/js/Daypilot/daypilot-navigator.src.js",
                "~/Content/js/Daypilot/daypilot-scheduler.src.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/select2").Include(
                "~/Content/js/select2.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker").Include(
                "~/Content/js/bootstrap-datetimepicker.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jqueryvalidate").Include(
                "~/Content/js/jquery.validate.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/moment").Include(
                "~/Content/js/moment.js"
                ));

            //Styles
            bundles.Add(new StyleBundle("~/bundles/css/bootstrap2.3").Include(
                "~/Content/css/bootstrap2.3.css"
                ));
            bundles.Add(new StyleBundle("~/bundles/css/bootstrap3.0").Include(
                "~/Content/css/bootstrap3.0.css"
                ));
            bundles.Add(new StyleBundle("~/bundles/css/colorbox").Include(
                "~/Content/css/colorbox.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/frontpage").Include(
                "~/Content/css/main.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/default").Include(
                "~/Content/css/default.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/fontawesome").Include(
                "~/Content/css/font-awesome.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/daypilot").Include(
                "~/Content/css/Daypilot/*_white.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/select2").Include(
               "~/Content/css/select2.css"
               ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-datetimepicker").Include(
               "~/Content/css/bootstrap-datetimepicker.css"
               ));
            
        }
    }
}
