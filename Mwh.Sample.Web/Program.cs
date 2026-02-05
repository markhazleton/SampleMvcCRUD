using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebSpark.Bootswatch;
using WebSpark.HttpClientUtility.RequestResult;
using Westwind.AspNetCore.Markdown;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v10",
        Title = "Sample CRUD API",
        Description = "A Sample CRUD for Employees",
        Contact = new OpenApiContact
        {
            Name = "Mark Hazleton",
            Url = new Uri("https://markhazleton.com")
        },
    });
});

// Database and data access services
builder.Services.AddDbContext<EmployeeContext>(opt => 
    opt.UseInMemoryDatabase("Employee"));
builder.Services.AddScoped<IEmployeeService, EmployeeDatabaseService>();
builder.Services.AddScoped<IEmployeeClient, EmployeeDatabaseClient>();

// Seed database during startup
using (var context = new EmployeeContext())
{
    SeedDatabase.DatabaseInitialization(context);
}

// HTTP and infrastructure services
builder.Services.AddHttpClient();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Application-specific services
builder.Services.AddScoped<IHttpRequestResultService, HttpRequestResultService>();
builder.Services.AddBootswatchThemeSwitcher();
builder.Services.AddMarkdown();

// Session and MVC configuration
builder.Services.AddSession();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Monitoring and diagnostics
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
});
builder.Services.AddHealthChecks();

// Problem details for standardized error responses
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Swagger configuration
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample CRUD API v10");
    options.DocumentTitle = "Sample CRUD API";
    options.InjectStylesheet("/swagger_custom/custom.css");
    options.RoutePrefix = "swagger";
});

// Request pipeline
app.UseMyHttpContext();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseBootswatchAll();

app.UseRouting();
app.UseAuthorization();
app.UseSession();

// Health check endpoint
app.MapHealthChecks("/health");

// Map controllers and pages
app.MapControllers();
app.MapRazorPages();

// Map MVC routes using modern endpoint routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "home",
    pattern: "home/index",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "index",
    pattern: "index.html",
    defaults: new { controller = "Home", action = "Index" });

// Markdown middleware
app.UseMarkdown();

app.Run();
