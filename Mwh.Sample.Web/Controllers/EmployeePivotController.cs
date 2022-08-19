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
        public EmployeePivotController(IEmployeeClient employeeClient, IConfiguration configuration) : base(configuration)
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
        /// GetEmployeeList
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetEmployeeList()
        {

            var paging = new PagingParameterModel
            {
                PageSize = 3000,
                PageNumber = 1
            };

            var list = await client.GetEmployeesAsync(paging, cts.Token).ConfigureAwait(false);
            return PartialView("_EmployeeList", list);
        }



    }
}
