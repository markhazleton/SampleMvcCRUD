
namespace Mwh.Sample.Web.Controllers;
/// <summary>
/// Home Controller
/// </summary>
public class HomeController : BaseController
{
    private readonly IEmployeeService employeeService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="svc"></param>
    public HomeController(IEmployeeService svc)
    {
        employeeService = svc;
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

