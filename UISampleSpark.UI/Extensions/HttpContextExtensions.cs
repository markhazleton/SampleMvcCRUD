namespace UISampleSpark.UI.Extensions;

/// <summary>
/// Extension methods for configuring and accessing HTTP context
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Configures the application to use the custom HTTP context accessor
    /// </summary>
    /// <param name="app">The application builder instance</param>
    /// <returns>The application builder instance for method chaining</returns>
    public static IApplicationBuilder UseMyHttpContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        return app;
    }
}
