using Microsoft.AspNetCore.Mvc;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// EmployeeReact
    /// </summary>
    public class EmployeeReactController : BaseController
    {
        /// <summary>
        /// EmployeeReact
        /// </summary>
        public EmployeeReactController() : base()
        {
        }

        /// <summary>
        /// Default Page for Employee React Controller
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index() { return View(); }
    }
}