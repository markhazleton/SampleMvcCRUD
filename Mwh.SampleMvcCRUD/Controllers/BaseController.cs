using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Clients;
using Mwh.Sample.Core.WebApi.Extensions;
using System.Threading;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// BaseController
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseController : Controller
    {
        public readonly EmployeeClient client;
        public readonly CancellationTokenSource cts;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController()
        {
            client = new EmployeeClient(MyHttpContext.AppBaseUrl, "Sample");
            cts = new CancellationTokenSource();
        }
    }
}
