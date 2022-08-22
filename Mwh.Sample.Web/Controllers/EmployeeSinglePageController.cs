namespace Mwh.Sample.Web.Controllers
{
    /// <summary>
    /// Single Page Javascript Example Controller
    /// </summary>
    public class EmployeeSinglePageController : BaseController
    {
        /// <summary>
        /// Constructor for Controller
        /// </summary>
        /// <param name="configuration"></param>
        public EmployeeSinglePageController(IConfiguration configuration) : base(configuration)
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
