using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// MvcEmployeeController
    /// </summary>
    public class MvcEmployeeController : BaseController
    {
        public MvcEmployeeController() : base()
        {
        }
        /// <summary>
        /// Default Page for MvcEmployeeController
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var list = await client.ListAsync(cts.Token).ConfigureAwait(false);
            return View(list);
        }

        /// <summary>
        /// View Employee Details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var emp = await client.FindByIdAsync(id, cts.Token).ConfigureAwait(false);
            return View(emp);
        }


        /// <summary>
        /// Load Page to Create A New Employee
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create() { return View(new EmployeeModel()); }


        /// <summary>
        /// Save New Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmployeeModel employee)
        {
            EmployeeResponse reqResponse;
            if (employee != null)
            {
                reqResponse = await client.SaveAsync(employee, cts.Token).ConfigureAwait(false);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Edit an employee by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var emp = await client.FindByIdAsync(id, cts.Token).ConfigureAwait(false);
            return View(emp);
        }


        /// <summary>
        /// Save Employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EmployeeModel employee)
        {
            EmployeeResponse reqResponse;
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                    reqResponse = await client.UpdateAsync(id, employee, cts.Token).ConfigureAwait(false);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Select an Employee to delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var emp = await client.FindByIdAsync(id, cts.Token).ConfigureAwait(false);
            return View(emp);
        }


        /// <summary>
        /// Delete Employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, EmployeeModel employee)
        {
            if (employee != null)
            {
                if (employee.EmployeeID == id)
                {
                    var result = await client.DeleteAsync(id, cts.Token).ConfigureAwait(false);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
