using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mwh.Sample.Core.WebApi.Models;
using System.Diagnostics;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    public class HomeController : BaseController
    {

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache) : base(logger, memoryCache)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Employee Single Page Applications
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/home/EmpSinglePage/")]
        public ActionResult EmpSinglePage()
        {
            return View();
        }


    }
}
