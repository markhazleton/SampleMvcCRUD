using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Core.WebApi.Models;
using System.Diagnostics;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : BaseController
    {
        public HomeController(ILogger<HomeController> logger) : base(logger) { }

        /// <summary>
        /// Employee Single Page Applications
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/home/EmpSinglePage/")]
        public ActionResult EmpSinglePage() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

        public IActionResult Index() { return View(); }

    }
}
