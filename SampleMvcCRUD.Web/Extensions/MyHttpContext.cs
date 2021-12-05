
namespace SampleMvcCRUD.Web.Extensions;

/// <summary>
/// 
/// </summary>
public class MyHttpContext
{
    private static IHttpContextAccessor m_httpContextAccessor;

    /// <summary>
    /// 
    /// </summary>
    public static HttpContext Current => m_httpContextAccessor.HttpContext;

    /// <summary>
    /// 
    /// </summary>
    public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

    internal static void Configure(IHttpContextAccessor contextAccessor)
    { m_httpContextAccessor = contextAccessor; }
}
