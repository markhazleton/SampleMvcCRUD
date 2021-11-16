
Console.WriteLine("Setup SQL Lite Database");

var options = new DbContextOptionsBuilder<EmployeeContext>()
    .UseSqlite(@"Data Source=EmployeeConsole.db")
    .EnableSensitiveDataLogging(true)
    .Options;
using var context = new EmployeeContext(options);
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

Console.WriteLine("Database is Setup");

var employeeService = new EmployeeDatabaseService(context);

var list = await employeeService.GetAsync(new CancellationToken());

Console.WriteLine($"Service List Count:{list?.Count()}");

Console.WriteLine("Create MOCK to get sample Employees");
var employeeMock = new EmployeeMock();

var responseList = new List<EmployeeResponse>();

Console.WriteLine("Add sample Employees to new database");
employeeMock.EmployeeCollection()?.ForEach(async emp =>
{
    responseList.Add(await employeeService.SaveAsync(emp));
});
Console.WriteLine($"Success List Count:{responseList?.Where(w=>w.Success==true).ToArray().Count()}");


list = await employeeService.GetAsync(new CancellationToken());
Console.WriteLine($"Service List Count:{list?.Count()}");

Console.WriteLine("Complete");


