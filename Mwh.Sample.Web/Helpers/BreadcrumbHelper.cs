using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mwh.Sample.Web.Helpers;

/// <summary>
/// Helper class for generating breadcrumb navigation
/// </summary>
public static class BreadcrumbHelper
{
    /// <summary>
    /// Generate breadcrumbs based on the current route
    /// </summary>
    public static List<(string Name, string Url)> GenerateBreadcrumbs(ViewContext viewContext)
    {
        var breadcrumbs = new List<(string Name, string Url)>();
        
        // Always start with Home
        breadcrumbs.Add(("Home", "/"));

        var areaName = viewContext.RouteData.Values["area"]?.ToString();
        var controllerName = viewContext.RouteData.Values["controller"]?.ToString();
        var actionName = viewContext.RouteData.Values["action"]?.ToString();
        var idValue = viewContext.RouteData.Values["id"]?.ToString();

        // Skip if we're already on the home page
        if (string.IsNullOrEmpty(controllerName) || controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(actionName) || actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            {
                return breadcrumbs;
            }
        }

        // Handle different controller patterns
        switch (controllerName?.ToLower())
        {
            case "mvcemployee":
                breadcrumbs.Add(("Employee Management", "/MvcEmployee"));
                AddActionBreadcrumb(breadcrumbs, actionName, idValue, "/MvcEmployee");
                break;

            case "employee":
                breadcrumbs.Add(("Employee AJAX", "/Employee"));
                AddActionBreadcrumb(breadcrumbs, actionName, idValue, "/Employee");
                break;

            case "employeesinglepage":
                breadcrumbs.Add(("Single Page App", "/EmployeeSinglePage"));
                AddActionBreadcrumb(breadcrumbs, actionName, idValue, "/EmployeeSinglePage");
                break;

            case "employeepivot":
                breadcrumbs.Add(("Employee Pivot", "/EmployeePivot"));
                AddActionBreadcrumb(breadcrumbs, actionName, idValue, "/EmployeePivot");
                break;

            default:
                // Handle other controllers generically
                if (!string.IsNullOrEmpty(controllerName))
                {
                    var controllerDisplayName = FormatControllerName(controllerName);
                    breadcrumbs.Add((controllerDisplayName, $"/{controllerName}"));
                    AddActionBreadcrumb(breadcrumbs, actionName, idValue, $"/{controllerName}");
                }
                break;
        }

        // Handle Razor Pages
        if (viewContext.ActionDescriptor.DisplayName?.Contains("Pages") == true || 
            viewContext.HttpContext.Request.Path.Value?.Contains("/EmployeeRazor") == true)
        {
            breadcrumbs.Clear();
            breadcrumbs.Add(("Home", "/"));
            breadcrumbs.Add(("Employee Razor Pages", "/EmployeeRazor"));
            
            var pageName = viewContext.HttpContext.Request.Path.Value?.Split('/').LastOrDefault();
            if (!string.IsNullOrEmpty(pageName) && !pageName.Equals("EmployeeRazor", StringComparison.OrdinalIgnoreCase))
            {
                AddPageBreadcrumb(breadcrumbs, pageName, idValue);
            }
        }

        return breadcrumbs;
    }

    /// <summary>
    /// Add action-specific breadcrumb
    /// </summary>
    private static void AddActionBreadcrumb(List<(string Name, string Url)> breadcrumbs, string? actionName, string? idValue, string basePath)
    {
        if (string.IsNullOrEmpty(actionName) || actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            return;

        switch (actionName.ToLower())
        {
            case "create":
                breadcrumbs.Add(("Create Employee", $"{basePath}/Create"));
                break;

            case "edit":
                breadcrumbs.Add(("Edit Employee", string.IsNullOrEmpty(idValue) ? $"{basePath}/Edit" : $"{basePath}/Edit/{idValue}"));
                break;

            case "details":
                breadcrumbs.Add(("Employee Details", string.IsNullOrEmpty(idValue) ? $"{basePath}/Details" : $"{basePath}/Details/{idValue}"));
                break;

            case "delete":
                breadcrumbs.Add(("Delete Employee", string.IsNullOrEmpty(idValue) ? $"{basePath}/Delete" : $"{basePath}/Delete/{idValue}"));
                break;

            default:
                var actionDisplayName = FormatActionName(actionName);
                breadcrumbs.Add((actionDisplayName, $"{basePath}/{actionName}"));
                break;
        }
    }

    /// <summary>
    /// Add page-specific breadcrumb for Razor Pages
    /// </summary>
    private static void AddPageBreadcrumb(List<(string Name, string Url)> breadcrumbs, string pageName, string? idValue)
    {
        switch (pageName.ToLower())
        {
            case "create":
                breadcrumbs.Add(("Create Employee", "/EmployeeRazor/Create"));
                break;

            case "edit":
                breadcrumbs.Add(("Edit Employee", string.IsNullOrEmpty(idValue) ? "/EmployeeRazor/Edit" : $"/EmployeeRazor/Edit/{idValue}"));
                break;

            case "details":
                breadcrumbs.Add(("Employee Details", string.IsNullOrEmpty(idValue) ? "/EmployeeRazor/Details" : $"/EmployeeRazor/Details/{idValue}"));
                break;

            case "delete":
                breadcrumbs.Add(("Delete Employee", string.IsNullOrEmpty(idValue) ? "/EmployeeRazor/Delete" : $"/EmployeeRazor/Delete/{idValue}"));
                break;

            default:
                var pageDisplayName = FormatPageName(pageName);
                breadcrumbs.Add((pageDisplayName, $"/EmployeeRazor/{pageName}"));
                break;
        }
    }

    /// <summary>
    /// Format controller name for display
    /// </summary>
    private static string FormatControllerName(string controllerName)
    {
        return controllerName switch
        {
            "MvcEmployee" => "MVC Employee",
            "EmployeeSinglePage" => "Single Page App",
            "EmployeePivot" => "Employee Pivot",
            _ => controllerName
        };
    }

    /// <summary>
    /// Format action name for display
    /// </summary>
    private static string FormatActionName(string actionName)
    {
        return actionName switch
        {
            "Create" => "Create",
            "Edit" => "Edit",
            "Details" => "Details", 
            "Delete" => "Delete",
            _ => actionName
        };
    }

    /// <summary>
    /// Format page name for display
    /// </summary>
    private static string FormatPageName(string pageName)
    {
        return pageName switch
        {
            "Create" => "Create",
            "Edit" => "Edit",
            "Details" => "Details",
            "Delete" => "Delete", 
            _ => pageName
        };
    }
}
