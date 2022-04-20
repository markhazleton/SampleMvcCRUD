namespace Mwh.Sample.Web.Extensions;

/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMyHttpContext(this IApplicationBuilder app)
    {
        MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        return app;
    }
}
