using ControlOrigins.Survey;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    public class SurveyController : Controller
    {
        private SoapClient.Services.SurveyService surveyService;
        public SurveyController()
        {
            surveyService = new SoapClient.Services.SurveyService(Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString());
        }
        public async Task<IActionResult> Index()
        {
            return View(await surveyService.GetSurveyCollection().ConfigureAwait(true));
        }
        [Route("/survey/{surveyId}")]
        public async Task<IActionResult> Edit(int surveyId)
        {
            var list = await surveyService.GetSurveyBySurveyId(surveyId).ConfigureAwait(true);
            return View("Edit",list);
        }
    }
}