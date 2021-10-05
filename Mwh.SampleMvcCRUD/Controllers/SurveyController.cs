using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SurveyController : Controller
    {
        private readonly ISurveyService surveyService;

        public SurveyController(ISurveyService SurveyService)
        { surveyService = SurveyService; }

        public async Task<IActionResult> Index()
        { return View(await surveyService.GetSurveyCollection().ConfigureAwait(false)); }

        [Route("/survey/{surveyId}")]
        public async Task<IActionResult> Edit(int surveyId)
        {
            var survey = await surveyService.GetSurveyBySurveyId(surveyId).ConfigureAwait(false);
            return View("Edit", survey);
        }
    }
}