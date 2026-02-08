namespace UISampleSpark.UI.Controllers.Api.Employee.v1
{
    /// <summary>
    /// Department API Controller
    /// </summary>
    [Route("api/department")]
    [ApiController]
    public class DepartmentApiController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;

        /// <summary>
        /// DepartmentApiController
        /// </summary>
        public DepartmentApiController(IEmployeeService employeeService) : base()
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Returns collection of all departments
        /// </summary>
        /// <param name="includeEmployees">Whether to include employees in the response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of departments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAsync(
            bool includeEmployees = false,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<DepartmentDto> departments = await _employeeService.GetDepartmentsAsync(includeEmployees, cancellationToken);

            return Ok(departments);
        }

        /// <summary>
        /// Returns a single department by identifier
        /// </summary>
        /// <param name="id">Department identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Department details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(typeof(ErrorResource), 404)]
        public async Task<ActionResult<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            DepartmentDto department = await _employeeService.FindDepartmentByIdAsync(id, cancellationToken);

            if (department is null)
            {
                return NotFound(new ErrorResource("Department not found"));
            }

            return Ok(department);
        }
    }
}
