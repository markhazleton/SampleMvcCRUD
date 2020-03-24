using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Runtime.Caching;
using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    public abstract class BaseController : Controller
    {

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController()
        {

        }

        /// <summary>
        /// 
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
