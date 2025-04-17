namespace Mwh.Sample.Web.Extensions;

/// <summary>
/// Provides static access to the current HTTP context throughout the application
/// </summary>
public static class MyHttpContext
{
    private static IHttpContextAccessor? m_httpContextAccessor;

    /// <summary>
    /// Gets the current HTTP context from the HTTP context accessor
    /// </summary>
    public static HttpContext? Current => m_httpContextAccessor?.HttpContext;

    /// <summary>
    /// Gets the base URL of the application based on the current HTTP request
    /// </summary>
    public static string AppBaseUrl => $"{Current?.Request.Scheme}://{Current?.Request.Host}{Current?.Request.PathBase}";

    internal static void Configure(IHttpContextAccessor contextAccessor)
    { m_httpContextAccessor = contextAccessor; }
}
