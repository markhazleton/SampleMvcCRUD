
namespace Mwh.Sample.Web.Controllers;

/// <summary>
/// BaseController
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class BaseController : Controller
{
    /// <summary>
    ///
    /// </summary>
    public readonly CancellationTokenSource cts;
    private readonly IConfiguration Config;

    /// <summary>
    /// BaseController
    /// </summary>
    protected BaseController(IConfiguration configuration)
    {
        cts = new CancellationTokenSource();
        this.Config = configuration;
    }
}
