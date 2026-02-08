namespace UISampleSpark.UI.Controllers;
/// <summary>
/// Home Controller
/// </summary>
public class HomeController : BaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    public HomeController(IConfiguration configuration, IWebHostEnvironment hostEnvironment) : base(configuration, hostEnvironment)
    {

    }
    /// <summary>
    /// Error Page Display
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

    /// <summary>
    /// Main Home Page
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }
}

