using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Models;
using System.Reflection;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// Base for all Api Controllers in this project
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public abstract class BaseApiController : Controller
    {
        /// <summary>
        /// GetApplicationStatus
        /// </summary>
        /// <returns></returns>
        protected ApplicationStatus GetApplicationStatus()
        {
            return new ApplicationStatus(Assembly.GetExecutingAssembly());
        }
    }
}