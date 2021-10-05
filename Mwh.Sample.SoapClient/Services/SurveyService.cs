using ControlOrigins.Survey;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.SoapClient.Services
{
    /// <summary>
    /// Class SurveyService.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        /// <summary>
        /// The client
        /// </summary>
        protected ServiceSoapClient _client;

        /// <summary>
        /// The user key
        /// </summary>
        protected string _UserKey;
        public SurveyService()
        {
            _client = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            _UserKey = Guid.Parse(Resources.ServiceGUID).ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyService"/> class.
        /// </summary>
        /// <param name="userKey">The user key.</param>
        public SurveyService(string userKey)
        {
            _client = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            _UserKey = userKey;
        }

        /// <summary>
        /// Gets the application by application identifier.
        /// </summary>
        /// <param name="ApplicationId">The application identifier.</param>
        /// <returns>ApplicationItem.</returns>
        public async Task<ApplicationItem> GetApplicationByApplicationID(int ApplicationId)
        {
            var request = new GetApplicationByApplicationIDRequest
            {
                Body = new GetApplicationByApplicationIDRequestBody(ApplicationId, _UserKey)
            };
            var x = await _client.GetApplicationByApplicationIDAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationByApplicationIDResult;
        }

        /// <summary>
        /// Gets the application item collection.
        /// </summary>
        /// <returns>ApplicationItem[].</returns>
        public async Task<ApplicationItem[]> GetApplicationItemCollection()
        {
            var request = new GetApplicationListRequest
            {
                Body = new GetApplicationListRequestBody(_UserKey)
            };
            var x = await _client.GetApplicationListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationListResult.ToArray();
        }

        /// <summary>
        /// Gets the application type by application type identifier.
        /// </summary>
        /// <param name="applicationTypeId">The application type identifier.</param>
        /// <returns>ApplicationTypeItem.</returns>
        public async Task<ApplicationTypeItem> GetApplicationTypeByApplicationTypeID(int applicationTypeId)
        {
            var request = new GetApplicationTypeRequest
            {
                Body = new GetApplicationTypeRequestBody(applicationTypeId, _UserKey)
            };
            var x = await _client.GetApplicationTypeAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationTypeResult;
        }

        /// <summary>
        /// Gets the application type by application type identifier.
        /// </summary>
        /// <param name="applicationType">Type of the application.</param>
        /// <returns>ApplicationTypeItem.</returns>
        public async Task<ApplicationTypeItem> GetApplicationTypeByApplicationTypeID(ApplicationTypeItem applicationType)
        {
            var request = new PutApplicationTypeRequest
            {
                Body = new PutApplicationTypeRequestBody(applicationType, _UserKey)
            };
            var x = await _client.PutApplicationTypeAsync(request).ConfigureAwait(true);
            return x.Body.PutApplicationTypeResult;
        }

        /// <summary>
        /// Gets the application type collection.
        /// </summary>
        /// <returns>ApplicationTypeItem[].</returns>
        public async Task<ApplicationTypeItem[]> GetApplicationTypeCollection()
        {
            var request = new GetApplicationTypeListRequest
            {
                Body = new GetApplicationTypeListRequestBody(_UserKey)
            };
            var x = await _client.GetApplicationTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationTypeListResult.ToArray();
        }

        /// <summary>
        /// Gets the company by company identifier.
        /// </summary>
        /// <param name="CompanyId">The company identifier.</param>
        /// <returns>CompanyItem.</returns>
        public async Task<CompanyItem> GetCompanyByCompanyId(int CompanyId)
        {
            var request = new GetCompanyRequest
            {
                Body = new GetCompanyRequestBody(CompanyId, _UserKey)
            };
            var x = await _client.GetCompanyAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyResult;
        }

        /// <summary>
        /// Gets the company collection.
        /// </summary>
        /// <returns>CompanyItem[].</returns>
        public async Task<CompanyItem[]> GetCompanyCollection()
        {
            var request = new GetCompanyListRequest
            {
                Body = new GetCompanyListRequestBody(_UserKey)
            };
            var x = await _client.GetCompanyListAsync(request).ConfigureAwait(true);
            return x.Body.GetCompanyListResult.ToArray();
        }

        /// <summary>
        /// Gets the survey by survey identifier.
        /// </summary>
        /// <param name="surveyId">The survey identifier.</param>
        /// <returns>SurveyItem.</returns>
        public async Task<SurveyItem> GetSurveyBySurveyId(int surveyId)
        {
            var request = new GetSurveyRequest
            {
                Body = new GetSurveyRequestBody(surveyId, _UserKey)
            };
            var x = await _client.GetSurveyAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyResult;
        }

        /// <summary>
        /// Gets the survey collection.
        /// </summary>
        /// <returns>SurveyItem[].</returns>
        public async Task<SurveyItem[]> GetSurveyCollection()
        {
            var request = new GetSurveysRequest
            {
                Body = new GetSurveysRequestBody(Array.Empty<SQLFilterClause>(), _UserKey)
            };
            var x = await _client.GetSurveysAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveysResult.ToArray();
        }

        /// <summary>
        /// Gets the type of the survey.
        /// </summary>
        /// <param name="surveyTypeId">The survey type identifier.</param>
        /// <returns>SurveyTypeItem.</returns>
        public async Task<SurveyTypeItem> GetSurveyType(int surveyTypeId)
        {
            var request = new GetSurveyTypeRequest
            {
                Body = new GetSurveyTypeRequestBody(surveyTypeId, _UserKey)
            };
            var x = await _client.GetSurveyTypeAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeResult;
        }

        /// <summary>
        /// Gets the survey type collection.
        /// </summary>
        /// <param name="surveyTypeId">The survey type identifier.</param>
        /// <returns>SurveyTypeItem[].</returns>
        public async Task<SurveyTypeItem[]> GetSurveyTypeCollection(int surveyTypeId)
        {
            var request = new GetSurveyTypeListRequest
            {
                Body = new GetSurveyTypeListRequestBody(_UserKey)
            };
            var x = await _client.GetSurveyTypeListAsync(request).ConfigureAwait(true);
            return x.Body.GetSurveyTypeListResult;
        }

        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns>ApplicationUserItem.</returns>
        public async Task<ApplicationUserItem> GetUserById(int Id)
        {
            var request = new GetApplicationUserByApplicationUserIDRequest
            {
                Body = new GetApplicationUserByApplicationUserIDRequestBody(_UserKey, Id)
            };
            var x = await _client.GetApplicationUserByApplicationUserIDAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationUserByApplicationUserIDResult;
        }

        /// <summary>
        /// Gets the user collection.
        /// </summary>
        /// <returns>ApplicationUserItem[].</returns>
        public async Task<ApplicationUserItem[]> GetUserCollection()
        {
            var request = new GetApplicationUserListRequest
            {
                Body = new GetApplicationUserListRequestBody(_UserKey)
            };
            var x = await _client.GetApplicationUserListAsync(request).ConfigureAwait(true);
            return x.Body.GetApplicationUserListResult.ToArray();
        }

        /// <summary>
        /// Puts the application.
        /// </summary>
        /// <param name="applicationItem">The application item.</param>
        /// <returns>ApplicationItem.</returns>
        public async Task<ApplicationItem> PutApplication(ApplicationItem applicationItem)
        {
            var request = new PutApplicationItemRequest
            {
                Body = new PutApplicationItemRequestBody()
            };
            var x = await _client.PutApplicationItemAsync(request).ConfigureAwait(true);
            return x.Body.PutApplicationItemResult;
        }

        /// <summary>
        /// Puts the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>CompanyItem.</returns>
        public async Task<CompanyItem> PutCompany(CompanyItem company)
        {
            var request = new PutCompanyRequest
            {
                Body = new PutCompanyRequestBody(company, _UserKey)
            };
            var x = await _client.PutCompanyAsync(request).ConfigureAwait(true);
            return x.Body.PutCompanyResult;
        }

        /// <summary>
        /// Puts the survey.
        /// </summary>
        /// <param name="survey">The survey.</param>
        /// <returns>SurveyItem.</returns>
        public async Task<SurveyItem> PutSurvey(SurveyItem survey)
        {
            var request = new PutSurveyItemRequest
            {
                Body = new PutSurveyItemRequestBody(survey, _UserKey)
            };
            var x = await _client.PutSurveyItemAsync(request).ConfigureAwait(true);
            return x.Body.PutSurveyItemResult;
        }

        /// <summary>
        /// Puts the user.
        /// </summary>
        /// <param name="userItem">The user item.</param>
        /// <returns>ApplicationUserItem.</returns>
        public async Task<ApplicationUserItem> PutUser(ApplicationUserItem userItem)
        {
            var request = new PutApplicationUserRequest
            {
                Body = new PutApplicationUserRequestBody(_UserKey, userItem)
            };
            var x = await _client.PutApplicationUserAsync(request).ConfigureAwait(true);
            return x.Body.PutApplicationUserResult;
        }
    }
}