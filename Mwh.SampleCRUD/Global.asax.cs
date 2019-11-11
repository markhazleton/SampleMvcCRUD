using log4net;
using log4net.Config;
using Newtonsoft.Json;
using SampleCRUD.App_Start;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

namespace SampleCRUD
{
    internal class ApiLogEntry
    {
        /// <summary>
        /// The (database) ID for the API log entry.
        /// </summary>
        public long ApiLogEntryId { get; set; }
        /// <summary>
        /// The application that made the request.
        /// </summary>
        ///
        public string Application { get; set; }
        /// <summary>
        /// The machine that made the request. 
        /// </summary>
        public string Machine { get; set; }
        /// <summary>
        /// The MachineName that made the request.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// The request content body.
        /// </summary>
        public string RequestContentBody { get; set; }
        /// <summary>
        /// The request content type. 
        /// </summary>
        public string RequestContentType { get; set; }
        /// <summary>
        /// The request headers.
        /// </summary>
        public string RequestHeaders { get; set; }

        /// <summary>
        /// Request Invoice Number (Session)
        /// </summary>
        public string RequestInvoiceNumber { get; set; }
        /// <summary>
        /// The IP address that made the request.
        /// </summary>
        public string RequestIpAddress { get; set; }
        /// <summary>
        /// The request method (GET, POST, etc). 
        /// </summary>
        public string RequestMethod { get; set; }
        /// <summary>
        /// The request route data. 
        /// </summary>
        public string RequestRouteData { get; set; }
        /// <summary>
        /// The request route template. 
        /// </summary>
        public string RequestRouteTemplate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long RequestTicks { get; set; }
        /// <summary>
        /// The request timestamp.
        /// </summary>
        public DateTime? RequestTimestamp { get; set; }
        /// <summary>
        /// The request URI. 
        /// </summary>
        public string RequestUri { get; set; }
        /// <summary>
        /// The response content body. 
        /// </summary>
        public string ResponseContentBody { get; set; }
        /// <summary>
        /// The response content type.
        /// </summary>
        public string ResponseContentType { get; set; }
        /// <summary>
        /// The response headers.
        /// </summary>
        public string ResponseHeaders { get; set; }
        /// <summary>
        /// The response status code.
        /// </summary>
        public int? ResponseStatusCode { get; set; }
        /// <summary>
        /// The response timestamp.
        /// </summary>
        public DateTime? ResponseTimestamp { get; set; }
        /// <summary>
        /// The user that made the request.
        /// </summary>
        public string User { get; set; }

    }

    internal class ApiLogHandler : DelegatingHandler
    {
        public ApiLogHandler()
        {

        }

        /// <summary>
        /// API Wite Log
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="MachineName"></param>
        /// <param name="AppName"></param>
        /// <param name="Segment"></param>
        /// <param name="Seconds"></param>
        /// <param name="ResponseSize"></param>
        /// <param name="UserID"></param>
        /// <param name="InvoiceNumber"></param>
        public void WriteLog(string Status, string MachineName, string AppName, string Segment, long Seconds, long ResponseSize, string UserID, string InvoiceNumber)
        {
            string sQuoteComma = ("\",\"");
            string sQuote = ("\"");
            try
            {
                using (StreamWriter sw = new StreamWriter(GetLogFileName(), true))
                    try
                    {
                        sw.WriteLine($"{sQuote}{DateTime.Now.ToShortDateString()}{sQuoteComma}{DateTime.Now.ToLongTimeString()}{sQuoteComma}{MachineName}{sQuoteComma}{AppName}{sQuoteComma}{Segment}{sQuoteComma}{Status}{sQuoteComma}{Seconds}{sQuoteComma}{ResponseSize}{sQuoteComma}{UserID}{sQuoteComma}{InvoiceNumber}{sQuote}");
                    }
                    catch
                    {
                        // Do Nothing
                    }
            }
            catch
            {
                // do nothing
            }
        }

        /// <summary>
        /// Send Async
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiLogEntry = CreateApiLogEntryWithRequestData(request);
            if (request.Content != null)
            {
                await request.Content.ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        apiLogEntry.RequestContentBody = task.Result;
                    }, cancellationToken).ConfigureAwait(false);
            }

            return await base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                    // Update the API log entry with response info
                    try
                    {
                        apiLogEntry.ResponseStatusCode = (int)response.StatusCode;
                        apiLogEntry.ResponseTimestamp = DateTime.Now;
                        if (response.Content != null)
                        {
                            apiLogEntry.ResponseContentBody = response.Content.ReadAsStringAsync().Result;
                            apiLogEntry.ResponseContentType = response.Content.Headers.ContentType.MediaType;
                        }
                        WriteLog(apiLogEntry.ResponseStatusCode.ToString()
                            , apiLogEntry.MachineName
                            , apiLogEntry.Application
                            , apiLogEntry.RequestUri
                            , (Environment.TickCount - apiLogEntry.RequestTicks)
                            , apiLogEntry.ResponseContentBody == null ? 0 : apiLogEntry.ResponseContentBody.Length
                            , apiLogEntry.User
                            , apiLogEntry.RequestInvoiceNumber
                            );
                    }
                    finally
                    {

                    }
                    return response;
                }, cancellationToken).ConfigureAwait(false);
        }

        private ApiLogEntry CreateApiLogEntryWithRequestData(HttpRequestMessage request)
        {
            var context = ((HttpContextBase)request.Properties["MS_HttpContext"]);
            if (context != null)
            {
                var routeData = request.GetRouteData();
                return new ApiLogEntry
                {
                    Application = context.Request.Headers["Application"],
                    MachineName = context.Request.Headers["MachineName"],
                    RequestInvoiceNumber = context.Request.Headers["InvoiceNumber"],
                    RequestTicks = Environment.TickCount,
                    User = context.Request.Headers["UserID"],
                    Machine = Environment.MachineName,
                    RequestContentType = context.Request.ContentType,
                    RequestRouteTemplate = routeData.Route.RouteTemplate,
                    RequestIpAddress = context.Request.UserHostAddress,
                    RequestMethod = request.Method.Method,
                    RequestTimestamp = DateTime.Now,
                    RequestUri = request.RequestUri.ToString()
                };
            }
            return new ApiLogEntry();

        }
        private string GetLogFileName()
        {
            string LogFileName = ConfigurationManager.AppSettings["APIClientLog"] ?? "D:\\Applications\\Logs\\default_APIClientLog.csv";
            return LogFileName.Replace(".csv", $"_{(DateTime.Now.ToString("yyyyMMdd", CultureInfo.GetCultureInfo("en-US")))}.csv");
        }

    }

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
        }
    }
    internal class BaseControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// Create Controller
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentNullException("controllerName");
            }
            var controllerType = GetControllerType(requestContext, controllerName);

            if (controllerType == null)
            {
                controllerName = "Home";
                controllerType = GetControllerType(requestContext, controllerName);
                requestContext.RouteData.Values["Controller"] = "Home";
                requestContext.RouteData.Values["action"] = "Index";
            }
            IController controller = GetControllerInstance(requestContext, controllerType);
            return controller;
        }
    }

    public class MvcApplication : HttpApplication
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Log.Error("Global.Application_Error");
            Log.Error($"Request.URL - {Request.RawUrl},\n Sender - {sender.ToString()},\n e - {e.ToString()}", ex);
            Response.Clear();
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            if(!(ex is HttpException httpException))
            {
                routeData.Values.Add("action", "Index");
            } else //It's an Http Exception, Let's handle it.
            {
                switch(httpException.GetHttpCode())
                {
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "NotFound");
                        break;

                    // Here you can handle Views to other error codes.
                    // I choose a General error template  
                    default:
                        routeData.Values.Add("action", "Index");
                        break;
                }
            }

            // Pass exception details to the target error View.
            routeData.Values.Add("error", ex);

            // Clear the error on server.
            Server.ClearError();

            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;
        }


        protected void Application_Start()
        {
            string LogName = $"sample-{Environment.MachineName.Substring(Environment.MachineName.Length - 4)}";
            GlobalContext.Properties["LogName"] = LogName;
            XmlConfigurator.Configure();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Use our local Base Controller Factory instead of the MVC Default
            ControllerBuilder.Current.SetControllerFactory(new BaseControllerFactory());

            // To avoid circular reference errors when returning related entities.
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;

            //var serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            //var contractResolver = (DefaultContractResolver)serializerSettings.ContractResolver;
            //contractResolver.IgnoreSerializableAttribute = true;

            //specify to use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            // Register our API Handler
            GlobalConfiguration.Configuration.MessageHandlers.Add(new ApiLogHandler());

            // Disable MVC Header stuff
            MvcHandler.DisableMvcResponseHeader = true;

            Log.Debug("Application Start");
        }
    }
}