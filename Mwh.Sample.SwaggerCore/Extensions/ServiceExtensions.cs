using Mwh.Sample.SwaggerCore.Options;

namespace Mwh.Sample.SwaggerCore.Extensions;

/// <summary>
/// Extensions to Services to configure Swagger/Open API
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        return services.AddVersioning().AddCustomSwagger();
    }

    private static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(setup =>
        {
            setup.DefaultApiVersion = new ApiVersion(1, 0);
            setup.AssumeDefaultVersionWhenUnspecified = true;
            setup.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
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
                "<a href='/'>Back To Home</a><p>Simple RESTful API built with ASP.NET 6.0 to show how to create RESTful services using a decoupled, maintainable architecture.</p> ",
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
            string xmlPath = string.Empty;

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
    private static IServiceCollection AddSwaggerVersioning(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // for further customization
            //options.OperationFilter<DefaultValuesFilter>();
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }
}
