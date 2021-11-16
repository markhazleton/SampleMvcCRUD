
namespace Mwh.Sample.Web.Controllers;

/// <summary>
/// Home Controller
/// </summary>
public class HomeController : BaseController
{
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

    public IActionResult Index()
    {
        return View();
    }
}

