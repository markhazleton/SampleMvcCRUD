using Microsoft.AspNetCore.Http;

namespace Mwh.Sample.Core.WebApi.Extensions
{
    public class MyHttpContext
    {
        private static IHttpContextAccessor m_httpContextAccessor;

        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        { m_httpContextAccessor = contextAccessor; }

    }
}
