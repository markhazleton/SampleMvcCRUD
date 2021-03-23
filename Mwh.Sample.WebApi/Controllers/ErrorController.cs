using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
    {
    /// <summary>
    /// Error Controller
    /// </summary>
    public class ErrorController : Controller
        {
        /// <summary>
        /// General Error Page for Most Errors
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
            {
            return View();
            }
        /// <summary>
        /// Error Page for File Not Found (404)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult NotFound()
            {
            return View();
            }
        }
    }
