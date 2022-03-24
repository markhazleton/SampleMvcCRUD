using Azure.Identity;
var builder = WebApplication.CreateBuilder(args);
var vaultUri = Environment.GetEnvironmentVariable("VaultUri");
if (vaultUri != null)
{
    var keyVaultEndpoint = new Uri(vaultUri);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Initialize the In Memory Database
SeedDatabase.ConfirmDatabaseCreation("Employee");
builder.Services.AddDbContext<EmployeeContext>(opt =>
    opt.UseInMemoryDatabase("Employee"));

builder.Services.AddHttpClient();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IEmployeeDB, EmployeeDB>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeClient, EmployeeRestClient>();
builder.Services.AddCustomSwagger();
builder.Services.AddSession();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCustomSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
