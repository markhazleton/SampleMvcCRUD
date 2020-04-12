using ControlOrigins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mwh.Sample.Blazor.ControlOrigins
{
	public class DataController : IDisposable
	{
		ServiceSoapClient myWS = new ServiceSoapClient(new ServiceSoapClient.EndpointConfiguration());

		public string myGUID
		{
			get
			{
				return Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString();
			}
		}

		public void Dispose()
		{
			if (myWS != null)
			{
				//myWS.Dispose();
				myWS = null;
			}
		}

		#region ApplicationUser / SiteUser
		public List<ApplicationUserItem> GetSiteUserList()
		{
			var reqBody = new GetSiteUserListRequestBody(myGUID);
			var req = new GetSiteUserListRequest(reqBody);
			return myWS.GetSiteUserList(req).Body.GetSiteUserListResult.ToList();
		}
		//public List<ApplicationUserItem> GetApplicationUserList()
		//{
		//	return myWS.GetApplicationUserList(myGUID).ToList();
		//}

		//public delegate void UserLoginResultsEventHandler(bool status);
		//private UserLoginResultsEventHandler UserLoginResultsEvent;

		//public event UserLoginResultsEventHandler UserLoginResults;
		//public ApplicationUserItem UserLogin(string UserName, string UserPass)
		//{
		//	return myWS.GetSiteUser(myGUID, UserName, UserPass);
		//}
		//public ApplicationUserItem CreateNewUser(string firstname, string lastname, string email, string password)
		//{
		//	return myWS.CreateNewUser(myGUID, firstname, lastname, email, password);
		//}
		//public bool Checkpassword(int userid, string pass)
		//{
		//	return myWS.Checkpassword(myGUID, userid, pass);
		//}
		//public bool updatepassword(int userid, string newpass)
		//{
		//	return myWS.updatepassword(myGUID, userid, newpass);
		//}
		//public bool RemoveUserFromApp(int UserID, int appID)
		//{
		//	return myWS.RemoveUserFromApp(myGUID, UserID, appID);
		//}
		//public bool UnRegisterUserFromApp(int UserID, int appid)
		//{
		//	return myWS.UnRegisterUserFromApp(myGUID, UserID, appid);
		//}
		//public ApplicationUserItem GetApplicationUserByApplicationUserID(int reqApplicationUserID)
		//{
		//	return myWS.GetApplicationUserByApplicationUserID(myGUID, reqApplicationUserID);
		//}
		//public void DeleteApplicationUser(ApplicationUserItem applicationUserItem)
		//{
		//	myWS.DeleteApplicationUser(myGUID, applicationUserItem);
		//}
		//public ApplicationUserItem UpdateApplicationUser(ApplicationUserItem Applicationuser)
		//{
		//	return myWS.PutApplicationUser(myGUID, Applicationuser);
		//}
		//public ApplicationUserRoleItem UpdateApplicationUserRole(ApplicationUserRoleItem myApplicationUserRole, string sReturn)
		//{
		//	return myWS.PutApplicationUserRole(myApplicationUserRole, myGUID);
		//}

		#endregion

		//#region SiteRole
		//public List<SiteRoleItem> GetSiteRoleList()
		//{
		//	return myWS.GetSiteRoleList(myGUID).ToList();
		//}
		//#endregion

		//#region Application Item / SiteApp / ApplicationUserRole
		//public List<ApplicationUserRoleItem> GetApplicationUserByApplicationID(int AppID)
		//{
		//	var myApp = GetApplicationByApplicationID(AppID);
		//	return myApp.ApplicationUserList.ToList();
		//}
		//public List<ApplicationItem> GetSiteAppListByUserID(int UserID)
		//{
		//	return myWS.GetSiteAppListByUserID(myGUID, UserID).ToList();
		//}
		//public ApplicationItem PutApplicationItem(ApplicationItem myOnlineSiteApp)
		//{
		//	return myWS.PutApplicationItem(myOnlineSiteApp, myGUID);
		//}
		//public bool SubscribeMeToApp(int userid, int appid)
		//{
		//	return myWS.SubscribeMeToApp(myGUID, userid, appid);
		//}
		//public ApplicationItem CloneSiteApp(int curAppID, string newAppName)
		//{
		//	return myWS.CloneSiteApp(myGUID, curAppID, newAppName);
		//}
		//public void DeleteApplication(ApplicationItem delApplicationItem)
		//{
		//	myWS.DeleteApplication(delApplicationItem, myGUID);
		//}
		//public ApplicationSurveyItem UpateApplicationSurvey(ApplicationSurveyItem myApplicationSurvey)
		//{
		//	return myWS.PutApplicationSurveyItem(myApplicationSurvey, myGUID);
		//}
		//public ApplicationItem GetApplicationByApplicationID(int reqApplicationID)
		//{
		//	return myWS.GetApplicationByApplicationID(reqApplicationID, myGUID);
		//}
		//public ApplicationItem UpdateApplication(ApplicationItem myApplication)
		//{
		//	return myWS.PutApplicationItem(myApplication, myGUID);
		//}
		//public List<ApplicationItem> GetApplicationList()
		//{
		//	return myWS.GetApplicationList(myGUID).ToList();
		//}
		//public void DeleteApplicationSurvey(ApplicationSurveyItem applicationSurveyItem)
		//{
		//	myWS.DeleteApplicationSurveyItem(applicationSurveyItem, myGUID);
		//}
		//public int DeleteApplicationUserRole(ApplicationUserRoleItem myApplicationUserRole)
		//{
		//	return myWS.DeleteApplicationUserRole(myApplicationUserRole, myGUID);
		//}
		//#endregion

		//#region Site Property
		//public bool SetProperty(int AppID, string PropertyKey, string value)
		//{
		//	return myWS.SetProperty(myGUID, AppID, PropertyKey, value);
		//}
		//public string GetPropertyValue(int AppID, string PropertyKey)
		//{
		//	return myWS.GetProperty(AppID, PropertyKey, myGUID).Value;
		//}
		//public PropertyItem GetProperty(int AppID, string PropertyKey)
		//{
		//	return myWS.GetProperty(AppID, PropertyKey, myGUID);
		//}
		//public PropertyItem PutProperty(PropertyItem myProperty)
		//{
		//	return myWS.PutProperty(myProperty, myGUID);
		//}
		//public bool DeleteProperty(int AppID, string propertyKey)
		//{
		//	return myWS.DeleteProperty(myGUID, AppID, propertyKey);
		//}
		//#endregion

		//#region Site Messages
		//public List<SiteMessageItem> GetSiteMessageList()
		//{
		//	return myWS.GetSiteMessageList(myGUID).ToList();
		//}
		//public SiteMessageItem GetSiteMessageByMessageID(int MessageId)
		//{
		//	return myWS.GetSiteMessageByMessageID(myGUID, MessageId);
		//}
		//public SiteMessageItem PutSiteMessage(SiteMessageItem myMessage)
		//{
		//	return myWS.PutSiteMessage(myGUID, myMessage);
		//}
		//public List<SiteMessageItem> GetUserSentMessages(int UserId)
		//{
		//	return myWS.GetUserSentMessages(myGUID, UserId).ToList();
		//}
		//public SiteMessageItem UserMessageOpened(SiteMessageItem myMessage)
		//{
		//	return myWS.UserMessageOpened(myGUID, myMessage);
		//}
		//public bool DeleteMessage(SiteMessageItem myMessage)
		//{
		//	return myWS.DeleteMessage(myGUID, myMessage);
		//}
		//public List<ApplicationUserItem> GetRelatedUsers(int UserId)
		//{
		//	return myWS.GetRelatedUsers(myGUID, UserId).ToList();
		//}

		//#endregion

		//#region Navigational Menu
		//public List<NavigationMenuItem> GetNavigationMenuList()
		//{
		//	return myWS.GetNavigationMenuList(myGUID).ToList();
		//}
		//public NavigationMenuItem PutNavigationMenuItem(NavigationMenuItem thisMenuItem)
		//{
		//	return myWS.PutNavigationMenuItem(myGUID, thisMenuItem);
		//}

		//public ApplicationItem SetDefaultNavigationItem(ApplicationItem reqSiteApp, int NavMenuItemID)
		//{
		//	return myWS.SetDefaultNavigationItem(myGUID, reqSiteApp, NavMenuItemID);
		//}

		//public bool DeleteNavigationMenuItem(NavigationMenuItem NavMenuItem)
		//{
		//	return myWS.DeleteNavigationMenuItem(myGUID, NavMenuItem);
		//}

		//#endregion

		//#region Lookups
		//public List<LookupItem> GetQuestionTypeList()
		//{
		//	return myWS.GetLookupList(LookupType.QuestionTypeList, myGUID).ToList();
		//}
		//public List<LookupItem> GetUnitOfMeasureList()
		//{
		//	return myWS.GetLookupList(LookupType.UnitOfMeasureList, myGUID).ToList();
		//}
		//public List<LookupItem> GetReviewRoleLevelList()
		//{
		//	return myWS.GetLookupList(LookupType.ReviewRoleLevelList, myGUID).ToList();
		//}
		//public List<LookupItem> GetSurveyResponseStatusList()
		//{
		//	return myWS.GetLookupList(com.controlorigins.ws.LookupType.SurveyResponseStatusList, myGUID).ToList();
		//}
		////Function GetSurveyTypes() As List(Of LookupItem)
		////    Return myWS.GetLookupList(com.controlorigins.ws.LookupType.SurveyTypeList, myGUID).ToList
		////End Function
		//public List<LookupItem> GetApplicationTypes()
		//{
		//	return myWS.GetLookupList(com.controlorigins.ws.LookupType.ApplicationTypeList, myGUID).ToList();
		//}
		//public List<LookupItem> GetSurveyLookupList()
		//{
		//	return myWS.GetLookupList(com.controlorigins.ws.LookupType.SurveyList, myGUID).ToList();
		//}
		//#endregion

		//#region Survey Response
		//public SurveyResponseItem[] GetSurveyResponseListByApplication(int appid, int appuserid)
		//{
		//	return myWS.GetSuveyResponseListByApplicationUserID(appid, appuserid, myGUID);
		//}
		//public SurveyResponseItem GetSurveyResponse(int SRID)
		//{
		//	return myWS.GetSurveyResponseItem(SRID, myGUID);
		//}
		//public SurveyResponseItem GetApplicationSurveyResponse_SelectBySurveyResponseID(int SurveyResponseID)
		//{
		//	return myWS.GetSurveyResponseItem(SurveyResponseID, myGUID) as SurveyResponseItem;
		//}

		//public List<SurveyResponseItem> GetSurveyResponsesByApplicationUserForInput(int ApplicationUserID, int ApplicationID)
		//{
		//	throw (new NotImplementedException());
		//}

		//public List<SurveyResponseItem> GetSurveyResponsesByApplicationUserForInput(int ApplicationUserID, int SurveyID, int ApplicationID)
		//{
		//	throw (new NotImplementedException());
		//}
		//public SurveyResponseStateItem UpdateSurveyResponseState(SurveyResponseItem dbSurveyResponse, string ActivityDescription, bool bEmailSent)
		//{
		//	throw (new NotImplementedException());
		//}
		//public int DeleteSurveyResponse(SurveyResponseItem mySurveyResponse)
		//{
		//	return myWS.DeleteSurveyResponseItem(mySurveyResponse, myGUID);
		//}

		//public SurveyResponseItem PutSurveyResponseItem(SurveyResponseItem thisSurveyResponse)
		//{
		//	return myWS.PutSurveyResponseItem(thisSurveyResponse, myGUID);
		//}
		////Function GetSurveyResponseCount(sWhere As String) As Integer
		////    Throw New NotImplementedException
		////End Function
		//public int ResetSurveyResponse(SurveyResponseItem thisSurveyResponse)
		//{
		//	return myWS.ResetSurveyResponseItem(thisSurveyResponse, myGUID);
		//}

		//#endregion

		//#region Survey
		//public SurveyItem GetSurveyBySurveyID(int SurveyID)
		//{
		//	return myWS.GetSurvey(SurveyID, myGUID);
		//}

		//public List<SurveyItem> GetSurveySummaries()
		//{
		//	return myWS.GetSurveySummaries(myGUID).ToList();
		//}
		//public SurveyItem UpdateSurvey(SurveyItem dbSurvey)
		//{
		//	return myWS.PutSurveyItem(dbSurvey, myGUID);
		//}
		//public bool DeleteSurvey(SurveyItem surveyItem)
		//{
		//	return myWS.DeleteSurveyItem(surveyItem, myGUID);
		//}
		//public SurveyItem ImportSurvey(SurveyItem newSurvey, int ApplicationID, int DefaultRoleID)
		//{
		//	throw (new NotImplementedException());
		//}
		//#endregion

		//#region Company
		//public List<CompanyItem> GetCompanyList()
		//{
		//	return myWS.GetCompanyList(myGUID).ToList();
		//}
		//public CompanyItem GetCompanyByCompanyID(int myCompanyID)
		//{
		//	return myWS.GetCompany(myCompanyID, myGUID);
		//}
		//public CompanyItem PutCompany(CompanyItem myCompany)
		//{
		//	return myWS.PutCompany(myCompany, myGUID);
		//}
		//public bool DeleteCompany(CompanyItem myCompany)
		//{
		//	return myWS.DeleteCompany(myCompany, myGUID);
		//}

		//#endregion

		//#region Application Chart
		//public List<ApplicationChartItem> GetApplicationChartList()
		//{
		//	return myWS.GetApplicationChartList(myGUID).ToList();
		//}
		//public ApplicationChartItem GetApplicationChartByApplicationChartID(int myApplicationChartID)
		//{
		//	return myWS.GetApplicationChart(myApplicationChartID, myGUID);
		//}
		//public ApplicationChartItem PutApplicationChart(ApplicationChartItem myApplicationChart)
		//{
		//	return myWS.PutApplicationChart(myApplicationChart, myGUID);
		//}
		//public bool DeleteApplicationChart(ApplicationChartItem myApplicationChart)
		//{
		//	return myWS.DeleteApplicationChart(myApplicationChart, myGUID);
		//}


		//#endregion
		//#region Application Type
		//public List<ApplicationTypeItem> GetApplicationTypeList()
		//{
		//	return myWS.GetApplicationTypeList(myGUID).ToList();
		//}
		//public ApplicationTypeItem GetApplicationTypeByApplicationTypeID(int myApplicationTypeID)
		//{
		//	return myWS.GetApplicationType(myApplicationTypeID, myGUID);
		//}
		//public ApplicationTypeItem PutApplicationType(ApplicationTypeItem myApplicationType)
		//{
		//	return myWS.PutApplicationType(myApplicationType, myGUID);
		//}
		//public bool DeleteApplicationType(ApplicationTypeItem myApplicationType)
		//{
		//	return myWS.DeleteApplicationType(myApplicationType, myGUID);
		//}

		//#endregion

		//#region Survey Type
		//public List<SurveyTypeItem> GetSurveyCategoryList()
		//{
		//	return myWS.GetSurveyCategoryList(myGUID).ToList();
		//}
		//public List<SurveyTypeItem> GetQuestionCategoryList()
		//{
		//	return myWS.GetQuestionCategoryList(myGUID).ToList();
		//}

		//public List<SurveyTypeItem> GetSurveyCategoryListByApplicationTypeID(int reqApplicationTypeID)
		//{
		//	return myWS.GetSurveyCategoryListByApplicationTypeID(reqApplicationTypeID, myGUID).ToList();
		//}
		//public List<SurveyTypeItem> GetQuestionCategoryListByParentCategoryID(int reqSurveyTypeID)
		//{
		//	if (reqSurveyTypeID == 0)
		//	{
		//		return new List<SurveyTypeItem>();
		//	}
		//	else
		//	{
		//		return (from i in myWS.GetQuestionCategoryList(myGUID) where i.ParentSurveyTypeID == reqSurveyTypeID select i).ToList();
		//	}
		//}


		//public SurveyTypeItem GetSurveyTypeBySurveyTypeID(int mySurveyTypeID)
		//{
		//	return myWS.GetSurveyType(mySurveyTypeID, myGUID);
		//}
		//public SurveyTypeItem PutSurveyType(SurveyTypeItem mySurveyType)
		//{
		//	return myWS.PutSurveyType(mySurveyType, myGUID);
		//}
		//public bool DeleteSurveyType(SurveyTypeItem mySurveytype)
		//{
		//	return myWS.DeleteSurveyType(mySurveytype, myGUID);
		//}

		//#endregion

		//public List<RoleItem> GetRoles()
		//{
		//	return myWS.GetRoles(myGUID).ToList();
		//}

		//#region Question Item Web Service Calls

		//public List<QuestionItem> GetQuestionList()
		//{
		//	SQLFilterClause[] Filters = new SQLFilterClause[0];
		//	return myWS.GetQuestions(Filters, myGUID).ToList();
		//}
		//public QuestionItem GetQuestionByQuestionID(object QuestionID)
		//{
		//	if (System.Convert.ToInt32(QuestionID) > 0)
		//	{
		//		return myWS.GetQuestionItem(QuestionID, myGUID);
		//	}
		//	else
		//	{
		//		return new QuestionItem { QuestionID = -1 };
		//	}
		//}
		//public bool DeleteQuestionByQuestionID(object QuestionID)
		//{
		//	var myQuestion = myWS.GetQuestionItem(QuestionID, myGUID);
		//	if (System.Convert.ToInt32(QuestionID) > 0)
		//	{
		//		return myWS.DeleteQuestionItem(myQuestion, myGUID);
		//	}
		//	else
		//	{
		//		return false;
		//	}
		//}
		//public QuestionItem GetQuestionByQuestionShortNM(string QuestionShortNM)
		//{
		//	if (!string.IsNullOrEmpty(QuestionShortNM))
		//	{
		//		return myWS.GetQuestionByQuestionShortNM(QuestionShortNM, myGUID);
		//	}
		//	else
		//	{
		//		return new QuestionItem { QuestionID = -1 };
		//	}
		//}


		//public QuestionItem PutQuestionItem(object myQuestionItem)
		//{
		//	return myWS.PutQuestionItem(myQuestionItem, myGUID);
		//}
		//#endregion


		//#region tblFiles
		//public List<tblFilesItem> GetFileList()
		//{
		//	return myWS.GettblFilesList(myGUID).ToList();
		//}
		//public tblFilesItem GetFileByID(int mytblFilesID)
		//{
		//	return myWS.GettblFiles(mytblFilesID, myGUID);
		//}
		//public tblFilesItem PutFile(tblFilesItem mytblFiles)
		//{
		//	return myWS.PuttblFiles(mytblFiles, myGUID);
		//}
		//public bool DeleteFile(tblFilesItem mytblFiles)
		//{
		//	return myWS.DeletetblFiles(mytblFiles, myGUID);
		//}

		//#endregion

	}
}
