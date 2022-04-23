
namespace Mwh.Sample.Web.Controllers.Api.Employee.v1
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/department")]
    public class DepartmentV1ApiController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;
        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public DepartmentV1ApiController(IEmployeeService employeeService) : base() { _employeeService = employeeService; }

        /// <summary>
        /// Returns collection of all employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAsync()
        {
            CancellationTokenSource cts = new();
            var employees = await _employeeService.GetDepartmentsAsync(cts.Token).ConfigureAwait(false);
            return Ok(employees);
        }
        /// <summary>
        /// Returns collection of all employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAsync(int id)
        {
            CancellationTokenSource cts = new();
            var dep = await _employeeService.FindDepartmentByIdAsync(id, cts.Token).ConfigureAwait(false);
            return Ok(dep);
        }
    }
}
