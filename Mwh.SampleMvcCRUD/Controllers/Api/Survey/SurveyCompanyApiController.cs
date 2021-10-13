using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.SampleMvcCRUD.Controllers.Api.Survey
{



    /// <summary>
    /// Company Api
    /// </summary>
    [Route("/api/survey/company")]
    public class SurveyCompanyApiController : BaseApiController
    {
        private readonly ISurveyService surveyService;
        public SurveyCompanyApiController(ISurveyService SurveyService) : base() { surveyService = SurveyService; }


        /// <summary>
        /// Get Company By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(CompanyItem), 200)]
        public async Task<CompanyItem> GetCompanyByCompanyIdAsync(int id)
        {
            return await surveyService.GetCompanyByCompanyId(id).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Company Collection
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CompanyItem>), 200)]
        public async Task<IEnumerable<CompanyItem>> GetCompanyCollectionAsync()
        {
            return await surveyService.GetCompanyCollection().ConfigureAwait(false);
        }

    }
}