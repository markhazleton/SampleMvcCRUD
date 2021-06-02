using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Mwh.Sample.Core.WebApi.Extensions;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseController : Controller
    {
        public readonly CancellationTokenSource cts;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController()
        {
            cts = new CancellationTokenSource();
        }
    }
}
