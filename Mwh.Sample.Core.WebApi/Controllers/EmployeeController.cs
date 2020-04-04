using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Repositories;

namespace Mwh.Sample.Core.WebApi.Controllers
{

    /// <summary>
    /// EmployeeController
    /// </summary>
    public class EmployeeController : BaseController
    {
        public EmployeeController(ILogger<HomeController> logger,IEmployeeDB employee) : base(logger, employee)
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
            return PartialView("_EmployeeDelete", employeeDB.Employee(id));
        }

        /// <summary>
        /// GetEmployeeEdit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEmployeeEdit(int id = 0)
        {
            return PartialView("_EmployeeEdit", employeeDB.Employee(id));
        }

        /// <summary>
        /// GetEmployeeList
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEmployeeList()
        {
            return PartialView("_EmployeeList", employeeDB.EmployeeCollection());
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