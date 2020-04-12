using ControlOrigins.Survey;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.SoapClient.Services
{
    public class SurveyService
    {
        protected ServiceSoapClient _client;
        protected string _UserKey;

        public SurveyService(string userKey)
        {
            // Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString()
            _client = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            _UserKey = userKey;
        }

        public async Task<ApplicationItem> GetApplicationByApplicationID(int ApplicationId)
        {
            GetApplicationByApplicationIDRequestBody reqBody = new GetApplicationByApplicationIDRequestBody();
            reqBody.reqApplicaitonID = ApplicationId;
            GetApplicationByApplicationIDRequest request = new GetApplicationByApplicationIDRequest(reqBody);
            var x = await _client.GetApplicationByApplicationIDAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationByApplicationIDResult;
        }

        public async Task<ApplicationItem[]> GetApplicationCollection()
        {
            GetApplicationListRequestBody reqBody = new GetApplicationListRequestBody();
            GetApplicationListRequest request = new GetApplicationListRequest(reqBody);
            var x = await _client.GetApplicationListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationListResult.ToArray();
        }

        public async Task<ApplicationTypeItem[]> GetApplicationTypeCollection()
        {
            GetApplicationTypeListRequestBody reqBody = new GetApplicationTypeListRequestBody();
            GetApplicationTypeListRequest request = new GetApplicationTypeListRequest(reqBody);
            var x = await _client.GetApplicationTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationTypeListResult.ToArray();
        }

        public async Task<CompanyItem[]> GetCompanyCollection()
        {
            GetCompanyListRequestBody reqBody = new GetCompanyListRequestBody();
            GetCompanyListRequest request = new GetCompanyListRequest(reqBody);
            var x = await _client.GetCompanyListAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyListResult.ToArray();
        }

        public async Task<SurveyItem[]> GetSurveyCollection()
        {
            GetSurveysRequest request = new GetSurveysRequest();
            GetSurveysRequestBody reqBody = new GetSurveysRequestBody();
            request.Body = new GetSurveysRequestBody(new SQLFilterClause[0], _UserKey);
            var x = await _client.GetSurveysAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveysResult.ToArray();
        }


        public async Task<SurveyTypeItem> GetSurveyType(int surveyTypeId)
        {
            GetSurveyTypeRequest request = new GetSurveyTypeRequest();
            GetSurveyTypeRequestBody reqBody = new GetSurveyTypeRequestBody();
            request.Body = new GetSurveyTypeRequestBody(surveyTypeId, _UserKey);
            var x = await _client.GetSurveyTypeAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeResult;
        }

        public async Task<SurveyTypeItem[]> GetSurveyTypeCollection(int surveyTypeId)
        {
            GetSurveyTypeListRequest request = new GetSurveyTypeListRequest();
            GetSurveyTypeListRequestBody reqBody = new GetSurveyTypeListRequestBody();
            request.Body = new GetSurveyTypeListRequestBody(_UserKey);
            var x = await _client.GetSurveyTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeListResult;
        }

        public async Task<ApplicationUserItem[]> GetUserCollection()
        {
            GetApplicationUserListRequestBody reqBody = new GetApplicationUserListRequestBody();
            GetApplicationUserListRequest request = new GetApplicationUserListRequest(reqBody);
            var x = await _client.GetApplicationUserListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationUserListResult.ToArray();
        }
    }
}
