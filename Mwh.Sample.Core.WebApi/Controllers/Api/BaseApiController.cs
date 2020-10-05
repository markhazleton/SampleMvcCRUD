
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// Base for all Api Controllers in this project
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class BaseApiController : Controller
    {
    }
}
