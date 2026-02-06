namespace Mwh.Sample.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class EmployeePivotController : BaseController
    {
        /// <summary>
        ///
        /// </summary>
        private readonly IEmployeeClient client;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeClient"></param>
        /// <param name="configuration"></param>
        /// <param name="hostEnvironment"></param>
        public EmployeePivotController(IEmployeeClient employeeClient, IConfiguration configuration,
        IWebHostEnvironment hostEnvironment) : base(configuration, hostEnvironment)
        {
            client = employeeClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GetEmployeeList - Returns partial view with HTML table (legacy)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetEmployeeList()
        {

            PagingParameterModel paging = new PagingParameterModel
            {
                PageSize = 3000,
                PageNumber = 1
            };

            IEnumerable<EmployeeDto> list = await client.GetEmployeesAsync(paging, cts.Token);
            return PartialView("_EmployeeList", list);
        }

        /// <summary>
        /// GetEmployeeData - Returns employee data as JSON for efficient pivot table loading
        /// </summary>
        /// <returns>JSON array of employee objects</returns>
        [HttpGet]
        public async Task<IActionResult> GetEmployeeData()
        {
            try
            {
                PagingParameterModel paging = new PagingParameterModel
                {
                    PageSize = 3000,
                    PageNumber = 1
                };

                IEnumerable<EmployeeDto> list = await client.GetEmployeesAsync(paging, cts.Token);

                // Transform to simple objects for PivotTable.js
                // Use friendly names (GenderName, DepartmentName) instead of enum values
                var pivotData = list.Select(e => new
                {
                    Name = e.Name,
                    Age = e.Age,
                    State = e.State,
                    Country = e.Country,
                    Department = e.DepartmentName ?? e.Department.ToString(),
                    Gender = e.GenderName ?? e.Gender.ToString()
                });

                return Json(pivotData);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



    }
}
