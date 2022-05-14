
namespace Mwh.Sample.Web.Controllers;
/// <summary>
/// Home Controller
/// </summary>
public class HomeController : BaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public HomeController(IConfiguration configuration) : base(configuration)
    {

    }
    /// <summary>
    /// Employee Single Page Applications
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("/home/EmpSinglePage/")]
    public ActionResult EmpSinglePage() { return View(); }

    /// <summary>
    /// Error Page Display
    /// </summary>
    /// <returns></returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

    /// <summary>
    /// Main Home Page
    /// </summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        return View();
    }
}

