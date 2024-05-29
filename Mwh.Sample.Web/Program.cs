using Microsoft.OpenApi.Models;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);

var vaultUri = Environment.GetEnvironmentVariable("VaultUri");
if (vaultUri != null)
{
    try
    {
        var keyVaultEndpoint = new Uri(vaultUri);
        builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v8",
        Title = "Sample CRUD API",
        Description = "A Sample CRUD for Employees",
        Contact = new OpenApiContact
        {
            Name = "Mark Hazleton",
            Url = new Uri("https://markhazleton.com")
        },
    });
});
// Initialize the In Memory Database
builder.Services.AddDbContext<EmployeeContext>(opt => opt.UseInMemoryDatabase("Employee"));
builder.Services.AddScoped<IEmployeeService, EmployeeDatabaseService>();
builder.Services.AddScoped<IEmployeeClient, EmployeeDatabaseClient>();
SeedDatabase.DatabaseInitialization(new EmployeeContext());

builder.Services.AddHttpClient();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMarkdown();
builder.Services.AddSession();
builder.Services.AddRazorPages();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
    .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample CRUD API v8");

    // Set the Swagger UI browser document title.
    options.DocumentTitle = "Sample CRUD API ";

    // Use a custom CSS stylesheet.
    options.InjectStylesheet("/swagger_custom/custom.css");

    // Set the Swagger UI to render at the application's root.
    options.RoutePrefix = "swagger";
});

app.UseMyHttpContext();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();
app.UseMarkdown();
app.UseStaticFiles();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
    routes.MapRoute(
        name: "home",
        template: "home/index",
        defaults: new { controller = "Home", action = "Index" });
    routes.MapRoute(
        name: "index",
        template: "index.html",
        defaults: new { controller = "Home", action = "Index" });
});

app.Run();
