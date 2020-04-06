using Mwh.Sample.Core.WebApi.Extensions;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi
{
    public abstract class ClientBase : IDisposable
    {
        /// <summary>
        /// The Name of the Client Application making the API Call
        /// </summary>
        public string AppName = string.Empty;
        /// <summary>
        /// The Root of the Domain with the API
        /// </summary>
        public string BaseAPIUrl = string.Empty;
        /// <summary>
        /// Public Flag for when the API Encounters and Error
        /// </summary>
        public bool IsError = false;
        /// <summary>
        /// The Status of the last request made from the API Client
        /// </summary>
        public string status = string.Empty;
        /// <summary>
        /// The User ID that is used to make the request, this defaults to Web Account
        /// </summary>
        public int UserID = 0;

        protected RestRequest restRequest;
        /// <summary>
        /// Lazy Client used to instantiate when needed rather than during constructor
        /// </summary>
        private Lazy<RestClient> _lazyClient;

        /// <summary>
        /// ClientBase constructor used to set Application Name and Base Url for requests
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="appName"></param>
        protected ClientBase(string baseUrl, string appName)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = MyHttpContext.AppBaseUrl;
            }
            if (string.IsNullOrEmpty(appName))
                baseUrl = "MISSING";

            AppName = appName;
            BaseAPIUrl = baseUrl.Trim('/');
            _lazyClient = new Lazy<RestClient>(() => GetRestClient());

            restRequest = new RestRequest();
            restRequest.AddHeader("UserID", UserID.ToString());
            restRequest.AddHeader("Application", AppName);
            restRequest.AddHeader("MachineName", Environment.MachineName);
        }

        /// <summary>
        /// Client Base constructor that adds Reservation User ID
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="appName"></param>
        /// <param name="userId"></param>
        protected ClientBase(string baseUrl, string appName, int userId)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = MyHttpContext.AppBaseUrl;
            }
            if (string.IsNullOrEmpty(appName))
                baseUrl = "MISSING";


            UserID = userId;
            AppName = appName;
            BaseAPIUrl = baseUrl.Trim('/');
            _lazyClient = new Lazy<RestClient>(() => GetRestClient());

            restRequest = new RestRequest();
            restRequest.AddHeader("UserID", UserID.ToString());
            restRequest.AddHeader("Application", AppName);
            restRequest.AddHeader("MachineName", Environment.MachineName);
        }


        /// <summary>
        /// Client Base Destructor part of the IDisposable implementation
        /// </summary>
        ~ClientBase() { Dispose(false); }

        /// <summary>
        /// Client Base Dispose Property part of the IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the HttpClient for this class,a lazy pattern is used to create an instance when needed but never more
        /// than a single instance
        /// </summary>
        /// <returns></returns>
        protected RestClient Client()
        {
            if (_lazyClient == null)
                throw new ObjectDisposedException("RestClient has been disposed");
            return _lazyClient.Value;
        }

        /// <summary>
        /// Client Base Dispose Property part of the IDisposable implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_lazyClient != null)
                if (disposing)
                    if (_lazyClient.IsValueCreated)
                    {
                        _lazyClient = null;
                    }
            // There are no unmanaged resources to release, but
            // if we add them, they need to be released here.
        }

        /// <summary>
        /// Execute HttpGet and return results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment"></param>
        /// <returns></returns>
        protected async Task<T> Get<T>(string urlSegment)
        {
            try
            {
                restRequest.Resource = urlSegment.TrimStart('/');
                restRequest.Method = Method.GET;
                IRestResponse response = await Client().ExecuteAsync(restRequest).ConfigureAwait(true);
                var jser = new JsonSerializer();
                var requestResponse = jser.Deserialize<T>(response);
                return requestResponse;
            }
            catch
            {
                IsError = true;
            }
            return default;
        }


        /// <summary>
        /// Return the HttpClient to be used to make requests to API
        /// </summary>
        /// <returns></returns>
        protected virtual RestClient GetRestClient()
        {
            RestClient client = new RestClient { BaseUrl = new Uri(BaseAPIUrl), Timeout = Int32.MaxValue };

            if (client == null)
            {
                return null;
            }
            return client;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        protected async Task<T> Post<T>(string urlSegment, object requestBody)
        {
            try
            {
                restRequest.Resource = urlSegment.TrimStart('/');
                restRequest.AddJsonBody(requestBody);
                restRequest.Method = Method.POST;
                IRestResponse response = await Client().ExecuteAsync(restRequest).ConfigureAwait(true);
                var jser = new JsonSerializer();
                var requestResponse = jser.Deserialize<T>(response);
                return requestResponse;
            }
            catch
            {
                IsError = true;
            }
            return default;
        }
        protected async Task<T> Delete<T>(string urlSegment)
        {
            try
            {
                restRequest.Resource = urlSegment.TrimStart('/');
                restRequest.Method = Method.DELETE;
                IRestResponse response = await Client().ExecuteAsync(restRequest).ConfigureAwait(true);
                var jser = new JsonSerializer();
                var requestResponse = jser.Deserialize<T>(response);
                return requestResponse;
            }
            catch
            {
                IsError = true;
            }
            return default;
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        protected async Task<T> Put<T>(string urlSegment, object requestBody)
        {
            try
            {
                restRequest.Resource = urlSegment.TrimStart('/');
                restRequest.AddJsonBody(requestBody);
                restRequest.Method = Method.PUT;
                IRestResponse response = await Client().ExecuteAsync(restRequest).ConfigureAwait(true);
                var jser = new JsonSerializer();
                var requestResponse = jser.Deserialize<T>(response);
                return requestResponse;
            }
            catch
            {
                IsError = true;
            }
            return default;
        }
    }
}
