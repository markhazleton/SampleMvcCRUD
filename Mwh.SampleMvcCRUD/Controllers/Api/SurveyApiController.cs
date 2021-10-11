using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    [Route("/api/survey/")]
    public class SurveyApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;

        public SurveyApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }

        [Route("application/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ApplicationItem), 200)]
        public async Task<ApplicationItem> GetApplicationByApplicationIDAsync(int id)
        {
            return await surveyService.GetApplicationByApplicationID(id).ConfigureAwait(false);
        }

        [Route("application/")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApplicationItem>), 200)]
        public async Task<IEnumerable<ApplicationItem>> GetApplicationItemCollectionAsync()
        {
            return await surveyService.GetApplicationItemCollection().ConfigureAwait(false);
        }
        [Route("applicationtype/")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApplicationTypeItem>), 200)]
        public async Task<IEnumerable<ApplicationTypeItem>> GetApplicationTypeAsync()
        {
            return await surveyService.GetApplicationTypeCollection().ConfigureAwait(false);
        }

        [Route("applicationtype/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ApplicationTypeItem), 200)]
        public async Task<ApplicationTypeItem> GetApplicationTypeByApplicationTypeIDAsync(int id)
        {
            return await surveyService.GetApplicationTypeByApplicationTypeID(id).ConfigureAwait(false);
        }
        /// <summary>
        /// Returns collection of all surveys
        /// </summary>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SurveyItem>), 200)]
        public async Task<SurveyItem> GetSurveyBySurveyId(int id)
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
        public async Task<IEnumerable<SurveyItem>> ListAsync()
        {
            return await surveyService.GetSurveyCollection().ConfigureAwait(false);
        }



    }
}