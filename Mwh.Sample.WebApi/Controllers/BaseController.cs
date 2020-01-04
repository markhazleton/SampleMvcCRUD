using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Runtime.Caching;
using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>Initializes a new instance of the <see cref="BaseController" /> class.</summary>
        protected BaseController()
        {

        }

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
