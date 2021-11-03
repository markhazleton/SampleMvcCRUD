using Mwh.Sample.Common.Clients;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.HttpClientFactory.Clients
{
    /// <summary>
    /// Class ClientBase.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class RestClientBase : IDisposable, IRestClientBase
    {

        /// <summary>
        /// Lazy Client used to instantiate when needed rather than during constructor
        /// </summary>
        private IHttpClientFactory _lazyClient;

        /// <summary>
        /// ClientBase constructor used to set Application Name and Base Url for requests
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="appName">Name of the application.</param>
        protected RestClientBase(string baseUrl, string appName, IHttpClientFactory clientFactory)
        {
            if (string.IsNullOrEmpty(appName))
                baseUrl = "MISSING";

            AppName = appName;
            BaseAPIUrl = baseUrl.Trim('/');
            _lazyClient = clientFactory;
        }

        /// <summary>
        /// Client Base Destructor part of the IDisposable implementation
        /// </summary>
        ~RestClientBase() { Dispose(false); }

        /// <summary>
        /// Gets the HttpClient for this class,a lazy pattern is used to create an instance when needed but never more
        /// than a single instance
        /// </summary>
        /// <returns>RestClient.</returns>
        /// <exception cref="ObjectDisposedException">RestClient has been disposed</exception>
        protected HttpClient Client()
        {
            if (_lazyClient == null)
                throw new ObjectDisposedException("RestClient has been disposed");
            return _lazyClient.CreateClient();
        }

        /// <summary>
        /// Deletes the specified URL segment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlSegment">The URL segment.</param>
        /// <returns>T.</returns>
        protected async Task<T> Delete<T>(string urlSegment)
        {
            HttpRequestMessage request = GetRequestMessage(urlSegment, HttpMethod.Delete);
            using var response = await Client().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserializing
                try
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    IsError = true;
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    IsError = true;
                    Console.WriteLine("Invalid JSON.");
                }
            }
            return default;
        }

        /// <summary>
        /// Client Base Dispose Property part of the IDisposable implementation
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
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
            HttpRequestMessage request = GetRequestMessage(urlSegment, HttpMethod.Get);
            using var response = await Client().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserializing
                try
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    IsError = true;
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    IsError = true;
                    Console.WriteLine("Invalid JSON.");
                }
            }
            return default;
        }

        private HttpRequestMessage GetRequestMessage(string urlSegment, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, new Uri($"{BaseAPIUrl}{urlSegment}"));
            request.Headers.TryAddWithoutValidation("UserID", UserID.ToString());
            request.Headers.TryAddWithoutValidation("Application", AppName);
            request.Headers.TryAddWithoutValidation("MachineName", Environment.MachineName);
            return request;
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
            HttpRequestMessage request = GetRequestMessage(urlSegment, HttpMethod.Post);
            request.Content = JsonContent.Create(requestBody);
            using var response = await Client().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserializing
                try
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    IsError = true;
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    IsError = true;
                    Console.WriteLine("Invalid JSON.");
                }
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
            HttpRequestMessage request = GetRequestMessage(urlSegment, HttpMethod.Put);
            request.Content = JsonContent.Create(requestBody);
            using var response = await Client().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserializing
                try
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    IsError = true;
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    IsError = true;
                    Console.WriteLine("Invalid JSON.");
                }
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