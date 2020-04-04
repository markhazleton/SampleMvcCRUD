using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Repositories;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    public abstract class BaseController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IEmployeeDB employeeDB;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController(ILogger<HomeController> logger, IEmployeeDB employee)
        {
            _logger = logger;
            employeeDB = employee;
        }
    }
}
