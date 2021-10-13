using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.SampleMvcCRUD.Controllers.Api.Survey
{






    /// <summary>
    /// Survey API
    /// </summary>
    [Route("/api/survey/")]
    public class SurveyApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;

        public SurveyApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }

        /// <summary>
        /// Returns collection of all surveys
        /// </summary>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SurveyItem>), 200)]
        public async Task<SurveyItem> GetSurveyBySurveyIdAsync(int id)
        {
            return await surveyService.GetSurveyBySurveyId(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns collection of all surveys
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IEnumerable<SurveyItem>), 200)]
        public async Task<IEnumerable<SurveyItem>> GetSurveyCollectionAsync()
        {
            return await surveyService.GetSurveyCollection().ConfigureAwait(false);
        }
    }
}