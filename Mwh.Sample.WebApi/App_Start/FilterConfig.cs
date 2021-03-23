using System.Web.Mvc;

namespace Mwh.Sample.WebApi
    {
    /// <summary>
    /// Filter Configuration
    /// </summary>
    public class FilterConfig
        {
        /// <summary>
        /// RegisterGlobalFilters
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
            {
            if (filters == null) return;
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
            }
        }
    }
