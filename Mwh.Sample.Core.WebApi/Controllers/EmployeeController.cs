using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Common.Repositories;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task<ActionResult> GetEmployeeDelete(int id = 0)
        {
            var employee = await client.FindByIdAsync(id, cts.Token).ConfigureAwait(true);
            return PartialView("_EmployeeDelete", employee);
        }

        /// <summary>
        /// GetEmployeeEdit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetEmployeeEdit(int id = 0)
        {
            var employee = await client.FindByIdAsync(id,cts.Token).ConfigureAwait(true);
            return PartialView("_EmployeeEdit", employee);
        }

        /// <summary>
        /// GetEmployeeList
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetEmployeeList()
        {
            var list = await client.ListAsync(cts.Token).ConfigureAwait(true);
            return PartialView("_EmployeeList", list);
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