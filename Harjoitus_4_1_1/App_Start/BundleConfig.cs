using System.Web;
using System.Web.Optimization;

namespace Harjoitus_4_1_1
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Popper must be loaded before Bootstrap (bootstrap.js uses global.Popper)
            bundles.Add(new ScriptBundle("~/bundles/popper").Include(
                        "~/Scripts/popper.js"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/Cerulean-bootstrap.css",
                      "~/Content/site.css"));

            // Cache-busting for bundle URLs (adds ?v=... automatically)
            bundles.UseCdn = false;
            BundleTable.Bundles.FileSetOrderList.Clear();
        }
    }
}
