
namespace Mwh.Sample.SwaggerCore.Extensions;

public static class AppBuilderExtensions
{
    /// <summary>
    /// UseSwaggerWithVersioning
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSwaggerWithVersioning(this IApplicationBuilder app, IConfiguration configuration)
    {
        IServiceProvider services = app.ApplicationServices;
        var provider = services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
            options.InjectStylesheet("/swagger_custom/custom.css");
            options.DocumentTitle = configuration.GetValue<string>("Swagger:ApiTitle");
        });
        return app;
    }

    /// <summary>
    /// Custom Swagger for App Builder
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                options.DocumentTitle = "API";
                options.InjectStylesheet("/swagger/custom.css");
            });
        return app;
    }
}
