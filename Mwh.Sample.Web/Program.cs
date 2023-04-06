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
builder.Services.AddSwaggerServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

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
app.UseMyHttpContext();
app.UseSwaggerWithVersioning(builder.Configuration);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMarkdown();
app.UseStaticFiles();
app.MapHealthChecks("/health");
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});
app.Run();
