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
            
        }
    }
}
