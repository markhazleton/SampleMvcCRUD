
namespace Mwh.Sample.Web.Controllers;

/// <summary>
/// EmployeeController
/// </summary>
public class EmployeeController : BaseController
{
    /// <summary>
    ///
    /// </summary>
    private readonly IEmployeeClient client;

    /// <summary>
    ///
    /// </summary>
    /// <param name="employeeClient"></param>
    public EmployeeController(IEmployeeClient employeeClient) : base()
    {
        client = employeeClient;
    }

    /// <summary>
    /// GetEmployeeDelete
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> GetEmployeeDelete(int id = 0)
    {
        var employee = await client.FindEmployeeByIdAsync(id, cts.Token).ConfigureAwait(false);
        return PartialView("_EmployeeDelete", employee);
    }

    /// <summary>
    /// GetEmployeeEdit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> GetEmployeeEdit(int id = 0)
    {
        var employee = await client.FindEmployeeByIdAsync(id, cts.Token).ConfigureAwait(false);
        return PartialView("_EmployeeEdit", employee);
    }

    /// <summary>
    /// GetEmployeeList
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> GetEmployeeList()
    {
        var list = await client.GetEmployeesAsync(cts.Token).ConfigureAwait(false);
        return PartialView("_EmployeeList", list);
    }

    /// <summary>
    /// Default Page for Employee Controller
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Index() { return View(); }
}
