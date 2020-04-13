using ControlOrigins.Survey;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.SoapClient.Services
{
    public class SurveyService: ISurveyService
    {
        protected ServiceSoapClient _client;
        protected string _UserKey;

        public SurveyService(string userKey)
        {
            _client = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            _UserKey = userKey;
        }

        public async Task<ApplicationItem> GetApplicationByApplicationID(int ApplicationId)
        {
            GetApplicationByApplicationIDRequestBody reqBody = new GetApplicationByApplicationIDRequestBody(ApplicationId,
                                                                                                            _UserKey);
            GetApplicationByApplicationIDRequest request = new GetApplicationByApplicationIDRequest(reqBody);
            var x = await _client.GetApplicationByApplicationIDAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationByApplicationIDResult;
        }

        public async Task<ApplicationItem[]> GetApplicationCollection()
        {
            GetApplicationListRequestBody reqBody = new GetApplicationListRequestBody(_UserKey);
            GetApplicationListRequest request = new GetApplicationListRequest(reqBody);
            var x = await _client.GetApplicationListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationListResult.ToArray();
        }

        public async Task<ApplicationTypeItem[]> GetApplicationTypeCollection()
        {
            GetApplicationTypeListRequestBody reqBody = new GetApplicationTypeListRequestBody(_UserKey);
            GetApplicationTypeListRequest request = new GetApplicationTypeListRequest(reqBody);
            var x = await _client.GetApplicationTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationTypeListResult.ToArray();
        }

        public async Task<CompanyItem> GetCompanyByCompanyId(int CompanyId)
        {
            GetCompanyRequestBody reqBody = new GetCompanyRequestBody(CompanyId, _UserKey);
            GetCompanyRequest request = new GetCompanyRequest(reqBody);
            var x = await _client.GetCompanyAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyResult;
        }

        public async Task<CompanyItem[]> GetCompanyCollection()
        {
            GetCompanyListRequestBody reqBody = new GetCompanyListRequestBody(_UserKey);
            GetCompanyListRequest request = new GetCompanyListRequest(reqBody);
            var x = await _client.GetCompanyListAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyListResult.ToArray();
        }

        public async Task<SurveyItem> GetSurveyBySurveyId(int surveyId)
        {
            GetSurveyRequest request = new GetSurveyRequest();
            request.Body = new GetSurveyRequestBody(surveyId, _UserKey);
            var x = await _client.GetSurveyAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyResult;
        }


        public async Task<SurveyItem[]> GetSurveyCollection()
        {
            GetSurveysRequest request = new GetSurveysRequest();
            request.Body = new GetSurveysRequestBody(new SQLFilterClause[0], _UserKey);
            var x = await _client.GetSurveysAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveysResult.ToArray();
        }


        public async Task<SurveyTypeItem> GetSurveyType(int surveyTypeId)
        {
            GetSurveyTypeRequest request = new GetSurveyTypeRequest();
            request.Body = new GetSurveyTypeRequestBody(surveyTypeId, _UserKey);
            var x = await _client.GetSurveyTypeAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeResult;
        }

        public async Task<SurveyTypeItem[]> GetSurveyTypeCollection(int surveyTypeId)
        {
            GetSurveyTypeListRequest request = new GetSurveyTypeListRequest();
            request.Body = new GetSurveyTypeListRequestBody(_UserKey);
            var x = await _client.GetSurveyTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeListResult;
        }

        public async Task<ApplicationUserItem> GetUserById(int Id)
        {
            GetApplicationUserByApplicationUserIDRequestBody reqBody = new GetApplicationUserByApplicationUserIDRequestBody(_UserKey,
                                                                                                                            Id);
            GetApplicationUserByApplicationUserIDRequest request = new GetApplicationUserByApplicationUserIDRequest(reqBody);
            var x = await _client.GetApplicationUserByApplicationUserIDAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationUserByApplicationUserIDResult;
        }

        public async Task<ApplicationUserItem[]> GetUserCollection()
        {
            GetApplicationUserListRequestBody reqBody = new GetApplicationUserListRequestBody(_UserKey);
            GetApplicationUserListRequest request = new GetApplicationUserListRequest(reqBody);
            var x = await _client.GetApplicationUserListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationUserListResult.ToArray();
        }

        public async Task<CompanyItem> PutCompany(CompanyItem company)
        {
            GetCompanyRequestBody reqBody = new GetCompanyRequestBody();
            GetCompanyRequest request = new GetCompanyRequest(reqBody);
            var x = await _client.GetCompanyAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyResult;
        }

        public async Task<SurveyItem> PutSurveyBySurveyId(SurveyItem survey)
        {
            PutSurveyItemRequest request = new PutSurveyItemRequest();
            request.Body = new PutSurveyItemRequestBody(survey, _UserKey);
            var x = await _client.PutSurveyItemAsync(request).ConfigureAwait(true);
            return x.Body.PutSurveyItemResult;
        }

        public async Task<ApplicationUserItem> PutUser(ApplicationUserItem userItem)
        {
            PutApplicationUserRequestBody reqBody = new PutApplicationUserRequestBody(_UserKey, userItem);
            PutApplicationUserRequest request = new PutApplicationUserRequest(reqBody);
            var x = await _client.PutApplicationUserAsync(request).ConfigureAwait(true);
            return x.Body.PutApplicationUserResult;
        }
    }
}
