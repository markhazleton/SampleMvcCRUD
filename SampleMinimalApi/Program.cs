using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mwh.Sample.Domain.Interfaces;
using Mwh.Sample.Domain.Models;
using Mwh.Sample.Repository.Models;
using Mwh.Sample.Repository.Services;
using SampleMinimalApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();
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
            Url = new Uri("https://markhazleton.controlorigins.com")
        },
    });
});

builder.Services.AddDbContext<EmployeeContext>(opt => opt.UseInMemoryDatabase("Employee"));
builder.Services.AddScoped<IEmployeeService, EmployeeDatabaseService>();
builder.Services.AddScoped<IEmployeeClient, EmployeeDatabaseClient>();
SeedDatabase.DatabaseInitialization(new EmployeeContext());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();


app.MapGet("/employees", async (IEmployeeService employeeService, CancellationToken token) =>
{
    var paging = new PagingParameterModel();
    var employees = await employeeService.GetEmployeesAsync(paging, token);
    return employees;
}).WithOpenApi();

app.MapGet("/employees/{id}", async (IEmployeeService employeeService, int id, CancellationToken token) =>
{
    var employee = await employeeService.FindEmployeeByIdAsync(id, token);
    if (employee == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(employee);
}).WithOpenApi();

app.MapGet("/departments", async (IEmployeeService employeeService, CancellationToken token) =>
{
    var employee = await employeeService.GetDepartmentsAsync(false, token);
    return employee;
}).WithOpenApi();

app.Run();

