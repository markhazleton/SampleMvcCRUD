using Microsoft.AspNetCore.Mvc;
using Mwh.Sample.SoapClient.Services;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SurveyController : Controller
    {
        private SurveyService surveyService;
        public SurveyController()
        { surveyService = new SurveyService(Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString()); }
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