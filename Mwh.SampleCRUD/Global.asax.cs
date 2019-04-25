using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SampleCRUD
{
    using SampleCRUD.App_Start;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
