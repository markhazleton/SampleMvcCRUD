using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// MvcEmployeeController
    /// </summary>
    public class MvcEmployeeController : BaseController
    {
        public MvcEmployeeController(ILogger<HomeController> logger, IEmployeeDB employee) : base(logger, employee)
        {
        }
        /// <summary>
        /// Default Page for MvcEmployeeController
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View(employeeDB.EmployeeCollection());
        }


        /// <summary>
        /// View Employee Details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View(employeeDB.Employee(id));
        }


        /// <summary>
        /// Load Page to Create A New Employee
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            return View(new EmployeeModel());
        }


        /// <summary>
        /// Save New Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeModel employee)
        {
            if (employee != null)
            {
                employee = employeeDB.Update(employee);
            }
            return RedirectToAction("Index");

        }


        /// <summary>
        /// Edit an employee by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(employeeDB.Employee(id));
        }


        /// <summary>
        /// Save Employee 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeModel employee)
        {
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                    employeeDB.Update(employee);
            }
            return RedirectToAction("Index");

        }


        /// <summary>
        /// Select an Employee to delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Delete(int id)
        {
            return View(employeeDB.Employee(id));
        }


        /// <summary>
        /// Delete Employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, EmployeeModel employee)
        {
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                    _ = employeeDB.Delete(employee.EmployeeID);
            }
            return RedirectToAction("Index");
        }
    }
}
