namespace Mwh.Sample.Web.Controllers;

/// <summary>
/// MvcEmployeeController
/// </summary>
public class MvcEmployeeController : BaseController
{
    private readonly IEmployeeClient client;

    /// <summary>
    /// Mvc Employee Controller Constructor
    /// </summary>
    /// <param name="employeeClient"></param>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    public MvcEmployeeController(IEmployeeClient employeeClient,
        IConfiguration configuration,
        IWebHostEnvironment hostEnvironment) : base(configuration, hostEnvironment)
    {
        client = employeeClient;
    }

    /// <summary>
    /// Load Page to Create A New Employee
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Create() { return View(); }

    /// <summary>
    /// Save New Employee
    /// </summary>
    /// <param name="employee"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(EmployeeDto? employee)
    {
        EmployeeResponse? reqResponse = null;
        if (employee != null)
        {
            reqResponse = await client.SaveAsync(employee, cts.Token);
        }
        if (reqResponse?.Success == false)
            return RedirectToAction("Index");

        return RedirectToAction("Edit", new { employee?.Id });

    }

    /// <summary>
    /// Select an Employee to delete
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Delete(int id)
    {
        var emp = await client.FindEmployeeByIdAsync(id, cts.Token);

        if (emp?.Success == false)
            return RedirectToAction("Index");

        return View(emp?.Resource);
    }

    /// <summary>
    /// Delete Employee
    /// </summary>
    /// <param name="id"></param>
    /// <param name="employee"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, EmployeeDto employee)
    {
        if (employee != null)
        {
            if (employee.Id == id)
            {
                _ = await client.DeleteAsync(id, cts.Token);
            }
        }
        return RedirectToAction("Index");
    }

    /// <summary>
    /// View Employee Details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Details(int id)
    {
        var emp = await client.FindEmployeeByIdAsync(id, cts.Token);
        if (emp?.Success == false)
            return RedirectToAction("Index");

        return View(emp?.Resource);
    }

    /// <summary>
    /// Edit an employee by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Edit(int id)
    {
        var emp = await client.FindEmployeeByIdAsync(id, cts.Token);

        if (emp?.Success == false)
            return RedirectToAction("Index");

        return View(emp?.Resource);
    }

    /// <summary>
    /// Save Employee
    /// </summary>
    /// <param name="id"></param>
    /// <param name="employee"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, EmployeeDto? employee)
    {
        if (employee is null)
            return RedirectToAction("Edit", new { employee?.Id });

        employee.ProfilePicture = UploadedFile(employee.ProfileImage);

        EmployeeResponse? reqResponse = null;
        if (employee != null)
        {
            if (employee.Id == id)
                reqResponse = await client.UpdateAsync(id, employee, cts.Token);
        }
        if (reqResponse?.Success == true)
            return RedirectToAction("Index");

        return RedirectToAction("Edit", new { employee?.Id });
    }

    /// <summary>
    /// Default Page for MvcEmployeeController
    /// </summary>
    /// <param name="paging"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Index(PagingParameterModel? paging)
    {
        if (paging == null)
            paging = new PagingParameterModel();

        var list = await client.GetEmployeesAsync(paging, cts.Token);

        return View(list);
    }
}

