
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    [Route("/status")]
    public class StatusController : BaseApiController
    {

        /// <summary>
        /// Returns Current Application Status
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), 200)]
        public ApplicationStatus GetAsync()
        {
            return GetApplicationStatus();
        }

    }
}
