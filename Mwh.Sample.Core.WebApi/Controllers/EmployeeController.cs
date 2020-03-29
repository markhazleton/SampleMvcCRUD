using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Mwh.Sample.Core.WebApi.Controllers
{

    /// <summary>
    /// EmployeeController
    /// </summary>
    public class EmployeeController : BaseController
    {
        public EmployeeController(ILogger<HomeController> logger, IMemoryCache memoryCache) : base(logger, memoryCache)
        {
        }

        /// <summary>
        /// GetEmployeeDelete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEmployeeDelete(int id = 0)
        {
            return PartialView("_EmployeeDelete", EmpDB.Employee(id));
        }

        /// <summary>
        /// GetEmployeeEdit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEmployeeEdit(int id = 0)
        {
            return PartialView("_EmployeeEdit", EmpDB.Employee(id));
        }

        /// <summary>
        /// GetEmployeeList
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEmployeeList()
        {
            return PartialView("_EmployeeList", EmpDB.EmployeeCollection());
        }

        /// <summary>
        /// Default Page for Employee Controller
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}