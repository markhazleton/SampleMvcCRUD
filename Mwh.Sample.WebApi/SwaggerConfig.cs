using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing.Constraints;
using System.Collections.Generic;
using Mwh.Sample.WebApi;
using Swagger.Net.Application;
using Swagger.Net;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Mwh.Sample.WebApi
{

}
