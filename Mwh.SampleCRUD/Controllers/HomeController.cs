namespace SampleCRUD.Controllers
{
    using SampleCRUD.Models;
    using System.Web.Mvc;

    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }
    }
}