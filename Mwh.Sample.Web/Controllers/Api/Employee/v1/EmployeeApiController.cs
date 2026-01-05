namespace Mwh.Sample.Web.Controllers.Api.Employee.v1;

/// <summary>
/// Employee Api Controller
/// </summary>
[Route("api/employee")]
[ApiController]
public class EmployeeApiController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// EmployeeApiController
    /// </summary>
    public EmployeeApiController(IEmployeeService employeeService) : base()
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Deletes a given employee according to an identifier.
    /// </summary>
    /// <remarks>
    /// Id must be a valid employee
    /// </remarks>
    /// <param name="id">Employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response for the request.</returns>
    [HttpDelete("{id}", Name = "DeleteEmployee")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EmployeeResponse result = await _employeeService.DeleteAsync(id, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new ErrorResource(result.Message));
        }

        return Ok(result);
    }

    /// <summary>
    /// Returns Single Employee according to an identifier.
    /// </summary>
    /// <param name="id">Employee identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee response</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EmployeeResponse result = await _employeeService.FindEmployeeByIdAsync(id, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Returns collection of all employees
    /// </summary>
    /// <param name="paging">Paging parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> ListAsync(
        [FromQuery] PagingParameterModel? paging = null, 
        CancellationToken cancellationToken = default)
    {
        paging ??= new PagingParameterModel();

        IEnumerable<EmployeeDto> employees = await _employeeService.GetEmployeesAsync(paging, cancellationToken);
        
        return Ok(employees);
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="employee">Employee data.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response for the request.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), 201)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult> PostAsync([FromBody] EmployeeDto? employee, CancellationToken cancellationToken = default)
    {
        if (employee is null)
            return BadRequest("Employee was null");

        EmployeeResponse result = await _employeeService.SaveAsync(employee, cancellationToken);

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response for the request.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(typeof(ErrorResource), 400)]
    public async Task<ActionResult<EmployeeResponse>> PutAsync(
        int id, 
        [FromBody] EmployeeDto employee, 
        CancellationToken cancellationToken = default)
    {
        EmployeeResponse result = await _employeeService.UpdateAsync(id, employee, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new ErrorResource(result.Message));
        }

        return Ok(result);
    }
}
