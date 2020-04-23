using ControlOrigins.Survey;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Blazor.ControlOrigins
{
	/// <summary>
	/// Class DataController.
	/// Implements the <see cref="System.IDisposable" />
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class DataController : IDisposable
	{
		/// <summary>
		/// Gets my unique identifier.
		/// </summary>
		/// <value>My unique identifier.</value>
		public string myGUID
		{
			get
			{
				return Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Gets the site user list.
		/// </summary>
		/// <returns>ApplicationUserItem[].</returns>
		public async Task<ApplicationUserItem[]> GetSiteUserList()
		{
			var myWS = new Mwh.Sample.SoapClient.Services.SurveyService(Guid.Parse("85AAA903-3C57-4FB0-B91D-B46633C7C637").ToString());
			return await myWS.GetUserCollection().ConfigureAwait(true);
		}
	}
}
