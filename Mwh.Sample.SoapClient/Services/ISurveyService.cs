using ControlOrigins.Survey;
using System.Threading.Tasks;

namespace Mwh.Sample.SoapClient.Services
{
    public interface ISurveyService
    {
        Task<ApplicationItem> GetApplicationByApplicationID(int ApplicationId);

        Task<ApplicationItem[]> GetApplicationCollection();

        Task<ApplicationTypeItem[]> GetApplicationTypeCollection();

        Task<CompanyItem> GetCompanyByCompanyId(int CompanyId);

        Task<CompanyItem[]> GetCompanyCollection();

        Task<SurveyItem> GetSurveyBySurveyId(int surveyId);

        Task<SurveyItem[]> GetSurveyCollection();

        Task<SurveyTypeItem> GetSurveyType(int surveyTypeId);

        Task<SurveyTypeItem[]> GetSurveyTypeCollection(int surveyTypeId);

        Task<ApplicationUserItem> GetUserById(int Id);

        Task<ApplicationUserItem[]> GetUserCollection();

        Task<CompanyItem> PutCompany(CompanyItem company);

        Task<SurveyItem> PutSurveyBySurveyId(SurveyItem survey);

        Task<ApplicationUserItem> PutUser(ApplicationUserItem userItem);
    }
}
