// ***********************************************************************
// Assembly         : Mwh.Sample.SoapClient
// Author           : mark
// Created          : 04-12-2020
//
// Last Modified By : mark
// Last Modified On : 04-19-2020
// ***********************************************************************
// <copyright file="SurveyService.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using ControlOrigins.Survey;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.SoapClient.Services
{
    public interface ISurveyService
    {
        /// <summary>
        /// Gets the application by application identifier.
        /// </summary>
        /// <param name="ApplicationId">The application identifier.</param>
        /// <returns>ApplicationItem.</returns>
        Task<ApplicationItem> GetApplicationByApplicationID(int ApplicationId);

        /// <summary>
        /// Gets the application item collection.
        /// </summary>
        /// <returns>ApplicationItem[].</returns>
        Task<ApplicationItem[]> GetApplicationItemCollection();

        /// <summary>
        /// Gets the application type by application type identifier.
        /// </summary>
        /// <param name="applicationType">Type of the application.</param>
        /// <returns>ApplicationTypeItem.</returns>
        Task<ApplicationTypeItem> GetApplicationTypeByApplicationTypeID(ApplicationTypeItem applicationType);

        /// <summary>
        /// Gets the application type by application type identifier.
        /// </summary>
        /// <param name="applicationTypeId">The application type identifier.</param>
        /// <returns>ApplicationTypeItem.</returns>
        Task<ApplicationTypeItem> GetApplicationTypeByApplicationTypeID(int applicationTypeId);

        /// <summary>
        /// Gets the application type collection.
        /// </summary>
        /// <returns>ApplicationTypeItem[].</returns>
        Task<ApplicationTypeItem[]> GetApplicationTypeCollection();

        /// <summary>
        /// Gets the company by company identifier.
        /// </summary>
        /// <param name="CompanyId">The company identifier.</param>
        /// <returns>CompanyItem.</returns>
        Task<CompanyItem> GetCompanyByCompanyId(int CompanyId);

        /// <summary>
        /// Gets the company collection.
        /// </summary>
        /// <returns>CompanyItem[].</returns>
        Task<CompanyItem[]> GetCompanyCollection();

        /// <summary>
        /// Gets the survey by survey identifier.
        /// </summary>
        /// <param name="surveyId">The survey identifier.</param>
        /// <returns>SurveyItem.</returns>
        Task<SurveyItem> GetSurveyBySurveyId(int surveyId);

        /// <summary>
        /// Gets the survey collection.
        /// </summary>
        /// <returns>SurveyItem[].</returns>
        Task<SurveyItem[]> GetSurveyCollection();

        /// <summary>
        /// Gets the type of the survey.
        /// </summary>
        /// <param name="surveyTypeId">The survey type identifier.</param>
        /// <returns>SurveyTypeItem.</returns>
        Task<SurveyTypeItem> GetSurveyType(int surveyTypeId);

        /// <summary>
        /// Gets the survey type collection.
        /// </summary>
        /// <param name="surveyTypeId">The survey type identifier.</param>
        /// <returns>SurveyTypeItem[].</returns>
        Task<SurveyTypeItem[]> GetSurveyTypeCollection(int surveyTypeId);

        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns>ApplicationUserItem.</returns>
        Task<ApplicationUserItem> GetUserById(int Id);

        /// <summary>
        /// Gets the user collection.
        /// </summary>
        /// <returns>ApplicationUserItem[].</returns>
        Task<ApplicationUserItem[]> GetUserCollection();

        /// <summary>
        /// Puts the application.
        /// </summary>
        /// <param name="applicationItem">The application item.</param>
        /// <returns>ApplicationItem.</returns>
        Task<ApplicationItem> PutApplication(ApplicationItem applicationItem);

        /// <summary>
        /// Puts the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>CompanyItem.</returns>
        Task<CompanyItem> PutCompany(CompanyItem company);

        /// <summary>
        /// Puts the survey.
        /// </summary>
        /// <param name="survey">The survey.</param>
        /// <returns>SurveyItem.</returns>
        Task<SurveyItem> PutSurvey(SurveyItem survey);

        /// <summary>
        /// Puts the user.
        /// </summary>
        /// <param name="userItem">The user item.</param>
        /// <returns>ApplicationUserItem.</returns>
        Task<ApplicationUserItem> PutUser(ApplicationUserItem userItem);
    }
}
