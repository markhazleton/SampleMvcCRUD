// Helpful URLs
// https://www.wardvanbesien.info/post/using-key-vault-when-developing-locally/ 
// 

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Make Sure Database is Created and Initialized every startup.
SeedDatabase.ConfirmDatabaseCreation(SampleMvcCRUD.Web.Properties.Resources.employee);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<EmployeeContext>(opt =>
    opt.UseInMemoryDatabase(SampleMvcCRUD.Web.Properties.Resources.employee));

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
    app.UseCustomSwagger();
}

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



