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
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddVersioning().AddCustomSwagger(configuration);
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
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(cfg =>
        {
            cfg.SwaggerDoc(configuration.GetValue<string>("SwaggerApiVersion"),
                new OpenApiInfo
                {
                    Title = configuration.GetValue<string>("SwaggerApiTitle"),
                    Version = configuration.GetValue<string>("SwaggerApiVersion"),
                    Description = $"<a href='/'>Back To Home</a><p>{configuration.GetValue<string>("SwaggerApiDescription")}</p>" ,
                    Contact = new OpenApiContact
                    {
                        Name = configuration.GetValue<string>("SwaggerUserProfile:Name"),
                        Url = new Uri(configuration.GetValue<string>("SwaggerUserProfile:Url")),
                        Email = configuration.GetValue<string>("SwaggerUserProfile:Email"),
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
