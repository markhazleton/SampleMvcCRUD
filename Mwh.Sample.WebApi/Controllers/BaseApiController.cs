using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Runtime.Caching;
using System.Web.Http;

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
        /// Employee Database Mock
        /// </summary>
        public static IEmployeeDB EmpDB
        {
            get
            {
                var cache = MemoryCache.Default;
                if (cache.Get("dataCache") == null)
                {
                    var cachePolicty = new CacheItemPolicy();
                    cachePolicty.AbsoluteExpiration = DateTime.Now.AddDays(1);
                    var data = new EmployeeMock();
                    cache.Add("dataCache", data, cachePolicty);
                    return data;
                }
                else
                {
                    IEmployeeDB data = (IEmployeeDB)cache.Get("dataCache");
                    return data;
                }
            }
        }
    }
}
