
namespace Mwh.Sample.Web.Extensions;

/// <summary>
/// Custom Swagger
/// </summary>
public static class CustomSwaggerExtensions
{
    /// <summary>
    /// Custom Swagger for Services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(cfg =>
        {
            cfg.SwaggerDoc("v1",
                           new OpenApiInfo
                           {
                               Title = "Mwh.Sample.WebAPI",
                               Version = "v1",
                               Description =
                "Simple RESTful API built with ASP.NET 6.0 to show how to create RESTful services using a decoupled, maintainable architecture. <br/><a href='/'>Back To Home</a>",
                               Contact =
                new OpenApiContact
                {
                    Name = "Mark Hazleton",
                    Url = new Uri("https://linkedin.com/in/markhazleton/"),
                    Email = "mark.hazleton@controlorigins.com",
                },
                               License = new OpenApiLicense { Name = "MIT", },
                           });

            var xmlFile = $"Mwh.Sample.Web.xml";
            string xmlPath = String.Empty;

            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot")))
            {
                xmlPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", xmlFile);
            }
            else
            {
                xmlPath = Path.Combine(AppContext.BaseDirectory, string.Empty, xmlFile);
            }
            cfg.IncludeXmlComments(xmlPath);
        });
        return services;
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
            });
        return app;
    }
}
