using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
    {

    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
        {
        /// <summary>
        /// Home Page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
            {
            ViewBag.Title = "Home Page";

            return View();
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
