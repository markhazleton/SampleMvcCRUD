using System.Web.Mvc;

namespace Mwh.Sample.WebApi.Controllers
{
    /// <summary>
    /// Error Controller
    /// </summary>
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }
    }
}
