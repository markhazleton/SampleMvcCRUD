using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mwh.Sample.WebApi
{
    /// <summary>
    /// WebApiApplication
    /// </summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// 
        /// </summary>
        protected void Application_Error()
        {
            HttpContext ctx = HttpContext.Current;
            ctx.Response.Clear();
            RequestContext rc = ((MvcHandler)ctx.CurrentHandler).RequestContext;
            rc.RouteData.Values["action"] = "Index";
            rc.RouteData.Values["controller"] = "Error";
            rc.RouteData.Values["id"] = string.Empty;

            IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
            IController controller = factory.CreateController(rc, "Error");
            controller.Execute(rc);
            ctx.Server.ClearError();
        }
        /// <summary>
        /// Application_Start
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
