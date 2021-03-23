using System.Web.Http;

namespace Mwh.Sample.WebApi
    {
    /// <summary>
    /// Web Api Configuration
    /// </summary>
    public static class WebApiConfig
        {
        /// <summary>
        /// Register Configuration
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
            {
            if (config == null) return;
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            }
        }
    }
