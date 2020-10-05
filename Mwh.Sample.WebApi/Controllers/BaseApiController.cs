using Mwh.Sample.Common.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Mwh.Sample.WebApi.Controllers
{

    /// <summary>
    /// BaseApiController
    /// </summary>
    public abstract class BaseApiController : ApiController
    {

        /// <summary>
        /// BaseApiController Constructor
        /// </summary>
        protected BaseApiController()
        {
        }

        /// <summary>
        ///
        /// </summary>
        protected IHttpActionResult LogRequest()
        {
            var actionMember = ((HttpActionDescriptor[])this.ControllerContext.RouteData.Route.DataTokens
                .ToList()
                .Where(w => string.Compare(w.Key.ToLower(), "actions", StringComparison.Ordinal) == 0)
                .FirstOrDefault()
                .Value)
                .FirstOrDefault();

            var myDict = new Dictionary<string, string>()
            {
                { "user", User.Identity.Name },
                { "RouteTemplate", Request.GetRouteData()?.Route?.RouteTemplate },
                { "ActionName", actionMember?.ActionName },
                { "ControllerName", actionMember?.ControllerDescriptor?.ControllerName }
            };

            return Ok(JsonConvert.SerializeObject(myDict));
        }


        /// <summary>
        /// Employee Database Mock
        /// </summary>
        public static IEmployeeDB EmpDB
        {
            get
            {
                var cache = MemoryCache.Default;
                if(cache.Get("dataCache") == null)
                {
                    var cachePolicty = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddDays(1) };
                    var data = new EmployeeMock();
                    cache.Add("dataCache", data, cachePolicty);
                    return data;
                } else
                {
                    IEmployeeDB data = (IEmployeeDB)cache.Get("dataCache");
                    return data;
                }
            }
        }
    }
}
