
namespace SampleMvcCRUD.Web.Controllers.Api;

/// <summary>
/// Status Controller
/// </summary>
[Route("/status")]
public class StatusController : BaseApiController
{
    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), 200)]
    public ApplicationStatus Get()
    {
        return GetApplicationStatus();
    }
}
