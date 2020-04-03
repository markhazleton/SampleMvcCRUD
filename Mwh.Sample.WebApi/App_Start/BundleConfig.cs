using System.Web.Optimization;

namespace Mwh.Sample.WebApi
{
    /// <summary>
    /// Bundle Configuration
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        ///     <link href="~/Content/bootstrap.css" rel="stylesheet" />
        ///         <script src = "~/Scripts/jquery-3.3.1.js" ></ script >
        ///         < script src="~/Scripts/bootstrap.js"></script>
        ///         <script src = "~/Scripts/jquery.unobtrusive-ajax.js" ></ script >

        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            if (bundles == null) return;
            //Scripts
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-3.4.1.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

            //CSS
            bundles.Add(new StyleBundle("~/bundles/style").Include("~/Content/bootstrap.css","~/Content/site.css"));
        }
    }
}
