using Mwh.Sample.WebApi;
using Swagger.Net.Application;
using System;
using System.Web;
using System.Web.Http;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Mwh.Sample.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "Mwh.Sample.WebApi");
                        c.AccessControlAllowOrigin("*");
                        c.IncludeAllXmlComments(thisAssembly, AppDomain.CurrentDomain.BaseDirectory);
                        c.IgnoreIsSpecifiedMembers();
                        c.DescribeAllEnumsAsStrings(camelCase: false);
                    })
                .EnableSwaggerUi(c =>
                    {
                        c.ShowExtensions(true);
                        c.SetValidatorUrl("https://online.swagger.io/validator");
                        c.UImaxDisplayedTags(100);
                        c.UIfilter("''");
                    });
        }
    }
}
