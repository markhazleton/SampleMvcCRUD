using System.Text.Json;
using Mwh.Sample.Domain.Models;

namespace Mwh.Sample.Web.Helpers;

/// <summary>
/// Helper class for generating structured data (JSON-LD) markup
/// </summary>
public static class StructuredDataHelper
{
    /// <summary>
    /// Generate Organization schema for the website
    /// </summary>
    public static string GenerateOrganizationSchema(string baseUrl)
    {
        var organization = new
        {
            context = "https://schema.org",
            type = "Organization",
            name = "Sample MVC CRUD",
            description = "A comprehensive ASP.NET MVC CRUD tutorial application demonstrating best practices for web development and Azure deployment.",
            url = baseUrl,
            logo = new
            {
                type = "ImageObject",
                url = $"{baseUrl}/favicon.svg"
            },
            sameAs = new[]
            {
                "https://github.com/markhazleton/SampleMvcCRUD",
                "https://markhazleton.com"
            },
            contactPoint = new
            {
                type = "ContactPoint",
                contactType = "Technical Support",
                url = "https://github.com/markhazleton/SampleMvcCRUD/issues"
            }
        };

        return JsonSerializer.Serialize(organization, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate WebSite schema for search functionality
    /// </summary>
    public static string GenerateWebSiteSchema(string baseUrl)
    {
        var website = new
        {
            context = "https://schema.org",
            type = "WebSite",
            name = "Sample MVC CRUD Tutorial",
            description = "Learn ASP.NET MVC CRUD operations with comprehensive examples and Azure deployment strategies.",
            url = baseUrl,
            potentialAction = new
            {
                type = "SearchAction",
                target = new
                {
                    type = "EntryPoint",
                    urlTemplate = $"{baseUrl}/search?q={{search_term_string}}"
                },
                queryInput = "required name=search_term_string"
            }
        };

        return JsonSerializer.Serialize(website, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate Employee Person schema
    /// </summary>
    public static string GenerateEmployeeSchema(EmployeeDto employee, string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(employee);
        var person = new
        {
            context = "https://schema.org",
            type = "Person",
            name = employee.Name,
            jobTitle = "Employee",
            gender = employee.GenderName,
            workLocation = new
            {
                type = "Place",
                address = new
                {
                    type = "PostalAddress",
                    addressRegion = employee.State,
                    addressCountry = employee.Country
                }
            },
            worksFor = new
            {
                type = "Organization",
                name = "Sample MVC CRUD",
                department = new
                {
                    type = "Organization",
                    name = employee.DepartmentName
                }
            },
            image = employee.ProfilePicture != null ? $"{baseUrl}/images/{employee.ProfilePicture}" : null,
            identifier = new
            {
                type = "PropertyValue",
                name = "Employee ID",
                value = employee.Id.ToString()
            }
        };

        return JsonSerializer.Serialize(person, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
    }

    /// <summary>
    /// Generate Tutorial/HowTo schema for learning pages
    /// </summary>
    public static string GenerateTutorialSchema(string title, string description, string baseUrl, string currentUrl)
    {
        var tutorial = new
        {
            context = "https://schema.org",
            type = "HowTo",
            name = title,
            description = description,
            image = $"{baseUrl}/favicon.svg",
            totalTime = "PT30M",
            estimatedCost = new
            {
                type = "MonetaryAmount",
                currency = "USD",
                value = "0"
            },
            supply = new[]
            {
                new
                {
                    type = "HowToSupply",
                    name = "Visual Studio or VS Code"
                },
                new
                {
                    type = "HowToSupply",
                    name = ".NET 9.0 SDK"
                }
            },
            tool = new[]
            {
                new
                {
                    type = "HowToTool",
                    name = "ASP.NET MVC Framework"
                }
            },
            step = new[]
            {
                new
                {
                    type = "HowToStep",
                    name = "Explore the CRUD Operations",
                    text = "Navigate through the different employee management approaches to understand MVC, Razor Pages, and SPA patterns.",
                    url = currentUrl
                }
            }
        };

        return JsonSerializer.Serialize(tutorial, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate BreadcrumbList schema
    /// </summary>
    public static string GenerateBreadcrumbSchema(List<(string Name, string Url)> breadcrumbs, string baseUrl)
    {
        var breadcrumbList = new
        {
            context = "https://schema.org",
            type = "BreadcrumbList",
            itemListElement = breadcrumbs.Select((breadcrumb, index) => new
            {
                type = "ListItem",
                position = index + 1,
                name = breadcrumb.Name,
                item = breadcrumb.Url.StartsWith("http") ? breadcrumb.Url : $"{baseUrl}{breadcrumb.Url}"
            }).ToArray()
        };

        return JsonSerializer.Serialize(breadcrumbList, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate SoftwareApplication schema for the demo application
    /// </summary>
    public static string GenerateSoftwareApplicationSchema(string baseUrl)
    {
        var application = new
        {
            context = "https://schema.org",
            type = "SoftwareApplication",
            name = "Sample MVC CRUD Application",
            description = "A demonstration ASP.NET MVC application showcasing CRUD operations, Azure deployment, and modern web development practices.",
            url = baseUrl,
            applicationCategory = "DeveloperApplication",
            operatingSystem = "Web Browser",
            offers = new
            {
                type = "Offer",
                price = "0",
                priceCurrency = "USD"
            },
            author = new
            {
                type = "Person",
                name = "Mark Hazleton",
                url = "https://markhazleton.com"
            },
            programmingLanguage = new[]
            {
                "C#",
                "JavaScript",
                "HTML",
                "CSS"
            },
            runtimePlatform = new[]
            {
                "ASP.NET Core",
                ".NET 9.0"
            }
        };

        return JsonSerializer.Serialize(application, new JsonSerializerOptions { WriteIndented = true });
    }
}
