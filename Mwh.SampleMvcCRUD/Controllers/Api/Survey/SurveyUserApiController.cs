using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.SampleMvcCRUD.Controllers.Api.Survey
{
    /// <summary>
    /// User Api
    /// </summary>
    [Route("/api/survey/user")]
    public class SurveyUserApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;
        public SurveyUserApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }


        /// <summary>
        /// Get User By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ApplicationUserItem), 200)]
        public async Task<ApplicationUserItem> GetUserByUserIdAsync(int id)
        {
            return await surveyService.GetUserById(id).ConfigureAwait(false);
        }
        /// <summary>
        /// Get User Collection
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApplicationUserItem>), 200)]
        public async Task<IEnumerable<ApplicationUserItem>> GetUserCollectionAsync()
        {
            return await surveyService.GetUserCollection().ConfigureAwait(false);
        }

    }
}