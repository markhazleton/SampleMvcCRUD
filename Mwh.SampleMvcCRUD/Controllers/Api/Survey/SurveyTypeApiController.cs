using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.SampleMvcCRUD.Controllers.Api.Survey
{
    /// <summary>
    /// Application Type Api
    /// </summary>
    [Route("/api/survey/surveytype")]
    public class SurveyTypeApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;
        public SurveyTypeApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SurveyTypeItem>), 200)]
        public async Task<IEnumerable<SurveyTypeItem>> GetSurveyTypeCollectionAsync()
        {
            return await surveyService.GetSurveyTypeCollection(0).ConfigureAwait(false);
        }
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(SurveyTypeItem), 200)]
        public async Task<SurveyTypeItem> GetSurveyTypeCollectionAsync(int id)
        {
            return await surveyService.GetSurveyType(id).ConfigureAwait(false);
        }

    }
}