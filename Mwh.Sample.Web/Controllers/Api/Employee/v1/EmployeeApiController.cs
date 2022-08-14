
namespace Mwh.Sample.Web.Controllers.Api.Employee.v1;

/// <summary>
/// Employee Api Controller
/// </summary>
[Route("api/employee")]
public class EmployeeApiController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    /// <summary>
    /// EmployeeApiController
    /// </summary>
    public EmployeeApiController(IEmployeeService employeeService) : base() { _employeeService = employeeService; }

    /// <summary>
    /// Deletes a given employee according to an identifier.
    /// </summary>
    /// /// <remarks>
    /// Id must be a valid employee
    /// </remarks>
    /// <param name="id">Employee identifier.</param>
    /// <returns>Response for the request.</returns>
    [HttpDelete("{id}", Name = "DeleteEmployee")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> DeleteAsync(int id)
    {
        CancellationTokenSource cts = new();
        var result = await _employeeService.DeleteAsync(id, cts.Token).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(new ErrorResource(result.Message));
        }
        return Ok(result);
    }

    /// <summary>
    /// Returns Single Employee according to an identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> FindByIdAsync(int id)
    {
        CancellationTokenSource cts = new();
        var result = await _employeeService.FindEmployeeByIdAsync(id, cts.Token).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Returns collection of all employees
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> ListAsync([FromQuery] PagingParameterModel? paging = null)
    {
        CancellationTokenSource cts = new();

        if (paging == null)
        {
            paging = new PagingParameterModel();
        }


        var employees = await _employeeService.GetEmployeesAsync(paging, cts.Token).ConfigureAwait(false);
        return Ok(employees);
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="employee">Employee data.</param>
    /// <returns>Response for the request.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), 201)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult> PostAsync([FromBody] EmployeeDto employee)
    {
        if (employee == null) return BadRequest("Employee was null");

        CancellationTokenSource cts = new();
        var result = await _employeeService.SaveAsync(employee, cts.Token).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(new ErrorResource(result.Message));
        }
        return Created($"/api/Employee/{employee.Id}", employee);
    }

    /// <summary>
    /// Updates an existing employee according to an identifier.
    /// </summary>
    /// <param name="id">Employee identifier.</param>
    /// <param name="employee">Updated employee data.</param>
    /// <returns>Response for the request.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> PutAsync(int id, [FromBody] EmployeeDto employee)
    {
        CancellationTokenSource cts = new();
        var result = await _employeeService.UpdateAsync(id, employee, cts.Token).ConfigureAwait(false);
        if (!result.Success)
        {
            return BadRequest(new ErrorResource(result.Message));
        }
        return Ok(result);
    }

}
