using System.Web;
using System.Web.Optimization;

namespace RVP
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/DataTables").Include(
                      "~/vendors/datatables.net/js/jquery.dataTables.js",
                      "~/vendors/datatables.net-bs/js/dataTables.bootstrap.js",
                      "~/vendors/datatables.net-responsive/js/dataTables.responsive.min.js",
                      "~/vendors/datatables.net-responsive-bs/js/responsive.bootstrap.js",
                      "~/vendors/datatables.net-buttons/js/buttons.print.min.js",
                      "~/vendors/datatables.net-buttons/js/dataTables.buttons.min.js"
                      ));

            /*** CSS ****/

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/font-awesome.css",
                      "~/Content/style.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/custom_css").Include( 
                      "~/Content/font-awesome.css",
                      "~/Content/style.css",
                      "~/Content/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Vendors/DataTables").Include(
                      "~/vendors/datatables.net-bs/css/dataTables.bootstrap.css",
                      "~/vendors/datatables.net-responsive-bs/css/responsive.bootstrap.min.css",
                      "~/vendors/datatables.net-buttons/css/buttons.dataTables.min.css"));
        }
    }
}
