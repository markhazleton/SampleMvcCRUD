using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Repositories;
using System;
using System.Runtime.Caching;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    public abstract class BaseController : Controller
    {
        private IMemoryCache _cache;
        private readonly ILogger<HomeController> _logger;
        private readonly IEmployeeDB employeeDB;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
            employeeDB = new EmployeeMock();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEmployeeDB EmpDB
        {
            get
            {
                IEmployeeDB cacheEntry;

                // Look for cache key.
                if (!_cache.TryGetValue(CacheKeys.Entry, out cacheEntry))
                {
                    // Key not in cache, so get data.
                    cacheEntry = new EmployeeMock();

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromSeconds(5000));

                    // Save data in cache.
                    _cache.Set(CacheKeys.Entry, cacheEntry, cacheEntryOptions);
                }
                return cacheEntry;
            }
        }

    }
}
