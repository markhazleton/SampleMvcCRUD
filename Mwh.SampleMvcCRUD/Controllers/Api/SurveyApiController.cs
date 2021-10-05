using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    [Route("/api/survey")]
    public class SurveyApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;
        public SurveyApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }

        /// <summary>
        /// Returns collection of all surveys
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SurveyItem>), 200)]
        public async Task<IEnumerable<SurveyItem>> ListAsync()
        {
            var employees = await surveyService.GetSurveyCollection().ConfigureAwait(false);
            return employees;
        }

        /// <summary>
        /// Returns collection of all surveys
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<SurveyItem>), 200)]
        public async Task<SurveyItem> GetSurveyBySurveyId(int id)
        {
            var result = await surveyService.GetSurveyBySurveyId(id).ConfigureAwait(false);
            return result;
        }
    }
}