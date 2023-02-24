namespace Mwh.Sample.Web.Controllers
{
    /// <summary>
    /// Single Page Javascript Example Controller
    /// </summary>
    public class EmployeeSinglePageController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="hostEnvironment"></param>
        public EmployeeSinglePageController(IConfiguration configuration, IWebHostEnvironment hostEnvironment) : base(configuration, hostEnvironment)
        {
        }

        /// <summary>
        /// Default Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}
