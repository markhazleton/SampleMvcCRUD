using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Mwh.Sample.Core.WebApi.Extensions
{
    public static class CustomSwaggerExtensions
    {
        /// <summary>
        ///
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
                    "Simple RESTful API built with ASP.NET 5.0 to show how to create RESTful services using a decoupled, maintainable architecture. <br/><a href='/'>Back To Home</a>",
                                   Contact =
                    new OpenApiContact
                    {
                        Name = "Mark Hazleton",
                        Url = new Uri("https://linkedin.com/in/markhazleton/"),
                        Email = "mark.hazleton@controlorigins.com"
                    },
                                   License = new OpenApiLicense { Name = "MIT", },
                               });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                cfg.IncludeXmlComments(xmlPath);
            });
            return services;
        }

        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee API");
                    options.DocumentTitle = "Employee API";
                });
            return app;
        }
    }
}