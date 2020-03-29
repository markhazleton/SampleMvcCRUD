using Microsoft.Extensions.Caching.Memory;
using Mwh.Sample.Common.Repositories;
using System;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseApiController
    /// </summary>
    public abstract class BaseApiController : System.Web.Http.ApiController
    {
        private IMemoryCache _cache;

        /// <summary>
        /// BaseApiController Constructor
        /// </summary>
        protected BaseApiController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        /// <summary>
        /// Employee Database Mock
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
                        .SetSlidingExpiration(TimeSpan.FromSeconds(3));

                    // Save data in cache.
                    _cache.Set(CacheKeys.Entry, cacheEntry, cacheEntryOptions);
                }
                return cacheEntry;
            }
        }
    }
}
