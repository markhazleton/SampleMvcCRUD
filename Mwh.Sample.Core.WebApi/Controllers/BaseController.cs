using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Repositories;
using System.Threading;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    public abstract class BaseController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IEmployeeDB employeeDB;
        public readonly SampleClient client;
        public readonly CancellationTokenSource cts;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController(ILogger<HomeController> logger, IEmployeeDB employee)
        {
            client = new SampleClient("https://localhost:44377/","Sample");
            cts = new CancellationTokenSource();

            _logger = logger;
            employeeDB = employee;
        }
    }
}
