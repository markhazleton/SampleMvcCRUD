using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<HomeController> _logger;
        public readonly SampleClient client;
        public readonly CancellationTokenSource cts;

        /// <summary>
        /// BaseController
        /// </summary>
        protected BaseController(ILogger<HomeController> logger)
        {
            client = new SampleClient(MyHttpContext.AppBaseUrl, "Sample");
            cts = new CancellationTokenSource();
            _logger = logger;
        }
    }
}
