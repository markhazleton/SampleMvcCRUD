
namespace Mwh.Sample.Web.Controllers.Api.Employee
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/department")]
    public class DepartmentApiController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;
        /// <summary>
        /// EmployeeApiController
        /// </summary>
        public DepartmentApiController(IEmployeeService employeeService) : base() { _employeeService = employeeService; }

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
            var dep = await _employeeService.FindDepartmentByIdAsync(id,cts.Token).ConfigureAwait(false);
            return Ok(dep);
        }
    }
}
