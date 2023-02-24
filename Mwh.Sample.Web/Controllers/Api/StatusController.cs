using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Mwh.Sample.Web.Controllers.Api;

/// <summary>
/// Status Controller
/// </summary>
[Route("/status")]
public class StatusController : BaseApiController
{
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiExplorer"></param>
    public StatusController(IApiDescriptionGroupCollectionProvider apiExplorer)
    {
        _apiExplorer = apiExplorer;
    }

    /// <summary>
    /// GetApplicationStatus
    /// </summary>
    /// <returns></returns>
    protected ApplicationStatus GetApplicationStatus()
    {
        return new ApplicationStatus(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [Route("explorer")]
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiExplorerModel>), 200)]
    public IActionResult ApiExplorer()
    {
        var result = _apiExplorer.ApiDescriptionGroups.Items.Select(s => new ApiExplorerModel()
        {
            GroupName = s.GroupName ?? "unknown",
            GroupItems = s.Items.Select(s => new ApiDescriptionModel()
            {
                RelativePath = s.RelativePath ?? "unknown"
            }).ToList()
        }).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApplicationStatus), 200)]
    public ApplicationStatus ApplicationStatus()
    {
        return GetApplicationStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    public class ApiExplorerModel
    {
        /// <summary>
        /// 
        /// </summary>
        public ApiExplorerModel()
        {
            GroupItems = new List<ApiDescriptionModel>();
            GroupName = string.Empty;

        }
        /// <summary>
        /// 
        /// </summary>
        public List<ApiDescriptionModel> GroupItems { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ApiDescriptionModel
    {
        /// <summary>
        /// 
        /// </summary>
        public ApiDescriptionModel()
        {
            RelativePath = string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        public string RelativePath { get; set; }
    }



}
