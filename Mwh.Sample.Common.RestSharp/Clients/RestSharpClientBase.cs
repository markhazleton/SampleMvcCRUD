using Mwh.Sample.Common.Clients;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.RestSharp.Clients
{
    /// <summary>
    /// Class ClientBase.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class RestSharpClientBase : IDisposable, IRestClientBase
    {

        /// <summary>
        /// Lazy Client used to instantiate when needed rather than during constructor
        /// </summary>
        private Lazy<RestClient> _lazyClient;

        /// <summary>
        /// The rest request
        /// </summary>
        private RestRequest restRequest;

        /// <summary>
        /// ClientBase constructor used to set Application Name and Base Url for requests
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="appName">Name of the application.</param>
        protected RestSharpClientBase(string baseUrl, string appName)
        {
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
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="appName">Name of the application.</param>
        /// <param name="userId">The user identifier.</param>
        protected RestSharpClientBase(string baseUrl, string appName, int userId)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = "MISSING";
            }
            if (string.IsNullOrEmpty(appName))
                appName = "MISSING";

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
        ~RestSharpClientBase() { Dispose(false); }

        /// <summary>
        /// Gets the HttpClient for this class,a lazy pattern is used to create an instance when needed but never more
        /// than a single instance
        /// </summary>
        /// <returns>RestClient.</returns>
        /// <exception cref="ObjectDisposedException">RestClient has been disposed</exception>
        protected RestClient Client()
        {
            if (_lazyClient == null)
                throw new ObjectDisposedException("RestClient has been disposed");
            return _lazyClient.Value;
        }

        /// <summary>
        /// Deletes the specified URL segment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment">The URL segment.</param>
        /// <returns>T.</returns>
        protected async Task<T> Delete<T>(string urlSegment)
        {
            try
            {
                restRequest = new RestRequest();
                restRequest.AddHeader("UserID", UserID.ToString());
                restRequest.AddHeader("Application", AppName);
                restRequest.AddHeader("MachineName", Environment.MachineName);
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
        /// Client Base Dispose Property part of the IDisposable implementation
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
        /// <param name="urlSegment">The URL segment.</param>
        /// <returns>T.</returns>
        protected async Task<T> GetAsync<T>(string urlSegment)
        {
            try
            {
                restRequest = new RestRequest();
                restRequest.AddHeader("UserID", UserID.ToString());
                restRequest.AddHeader("Application", AppName);
                restRequest.AddHeader("MachineName", Environment.MachineName);
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
        /// <returns>RestClient.</returns>
        protected virtual RestClient GetRestClient()
        {
            RestClient client = new() { BaseUrl = new Uri(BaseAPIUrl), Timeout = Int32.MaxValue };

            if (client == null)
            {
                return null;
            }
            return client;
        }

        /// <summary>
        /// Posts the specified URL segment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment">The URL segment.</param>
        /// <param name="requestBody">The request body.</param>
        /// <returns>T.</returns>
        protected async Task<T> Post<T>(string urlSegment, object requestBody)
        {
            try
            {
                restRequest = new RestRequest();
                restRequest.AddHeader("UserID", UserID.ToString());
                restRequest.AddHeader("Application", AppName);
                restRequest.AddHeader("MachineName", Environment.MachineName);
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

        /// <summary>
        /// Puts the specified URL segment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment">The URL segment.</param>
        /// <param name="requestBody">The request body.</param>
        /// <returns>T.</returns>
        protected async Task<T> Put<T>(string urlSegment, object requestBody)
        {
            try
            {
                restRequest = new RestRequest();
                restRequest.AddHeader("UserID", UserID.ToString());
                restRequest.AddHeader("Application", AppName);
                restRequest.AddHeader("MachineName", Environment.MachineName);
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

        /// <summary>
        /// Client Base Dispose Property part of the IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The Name of the Client Application making the API Call
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// The Root of the Domain with the API
        /// </summary>
        public string BaseAPIUrl { get; set; } = string.Empty;

        /// <summary>
        /// Public Flag for when the API Encounters and Error
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// The Status of the last request made from the API Client
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The User ID that is used to make the request, this defaults to Web Account
        /// </summary>
        public int UserID { get; set; }
    }
}