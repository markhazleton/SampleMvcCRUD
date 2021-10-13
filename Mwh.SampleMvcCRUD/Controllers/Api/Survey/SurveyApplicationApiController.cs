using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.SampleMvcCRUD.Controllers.Api.Survey
{
    /// <summary>
    /// Application Api
    /// </summary>
    [Route("/api/survey/application")]
    public class SurveyApplicationApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;
        public SurveyApplicationApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }

        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ApplicationItem), 200)]
        public async Task<ApplicationItem> GetApplicationByApplicationIDAsync(int id)
        {
            return await surveyService.GetApplicationByApplicationID(id).ConfigureAwait(false);
        }

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApplicationItem>), 200)]
        public async Task<IEnumerable<ApplicationItem>> GetApplicationItemCollectionAsync()
        {
            return await surveyService.GetApplicationItemCollection().ConfigureAwait(false);
        }


    }
}